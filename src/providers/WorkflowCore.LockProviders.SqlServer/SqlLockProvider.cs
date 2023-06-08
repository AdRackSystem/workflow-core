using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WorkflowCore.Interface;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using WorkflowCore.Persistence.EntityFramework.Services;

namespace WorkflowCore.LockProviders.SqlServer
{
    public class SqlLockProvider : IDistributedLockProvider
    {
        private const string Prefix = "wfc";

        private readonly WorkflowDbContext _db;
        private readonly ILogger _logger;
        private readonly AutoResetEvent _mutex = new AutoResetEvent(true);

        public SqlLockProvider(WorkflowDbContext db, ILogger<SqlLockProvider> logger)
        {
            _db = db;
            _logger = logger;
        }


        public async Task<bool> AcquireLock(string Id, CancellationToken cancellationToken)
        {
            if (_mutex.WaitOne())
            {
                try
                {
                    await using var connection = (SqlConnection)_db.Database.GetDbConnection();
                    await connection.OpenAsync(cancellationToken);
                    try
                    {
                        var cmd = connection.CreateCommand();
                        cmd.CommandText = "sp_getapplock";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Resource", $"{Prefix}:{Id}");
                        cmd.Parameters.AddWithValue("@LockOwner", $"Session");
                        cmd.Parameters.AddWithValue("@LockMode", $"Exclusive");
                        cmd.Parameters.AddWithValue("@LockTimeout", 0);
                        var returnParameter = cmd.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        await cmd.ExecuteNonQueryAsync(cancellationToken);
                        var result = Convert.ToInt32(returnParameter.Value);

                        switch (result)
                        {
                            case -1:
                                _logger.LogDebug($"The lock request timed out for {Id}");
                                break;
                            case -2:
                                _logger.LogDebug($"The lock request was canceled for {Id}");
                                break;
                            case -3:
                                _logger.LogDebug($"The lock request was chosen as a deadlock victim for {Id}");
                                break;
                            case -999:
                                _logger.LogError($"Lock provider error for {Id}");
                                break;
                        }
                        if (result >= 0)
                        {
                            return true;
                        }
                        else
                        {
                            connection.Close();
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        connection.Close();
                        throw;
                    }
                }
                finally
                {
                    _mutex.Set();
                }
            }
            return false;
        }

        public async Task ReleaseLock(string Id)
        {
            if (_mutex.WaitOne())
            {
                try
                {
                    await using var connection = (SqlConnection)_db.Database.GetDbConnection();

                    var cmd = connection.CreateCommand();
                    cmd.CommandText = "sp_releaseapplock";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Resource", $"{Prefix}:{Id}");
                    cmd.Parameters.AddWithValue("@LockOwner", $"Session");
                    var returnParameter = cmd.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;

                    await cmd.ExecuteNonQueryAsync();
                    var result = Convert.ToInt32(returnParameter.Value);

                    if (result < 0)
                        _logger.LogError($"Unable to release lock for {Id}");
                }
                finally
                {
                    _mutex.Set();
                }
            }
        }

        public Task Start() => Task.CompletedTask;
        
        public Task Stop() => Task.CompletedTask;
        
    }
}
