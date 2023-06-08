using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Persistence.EntityFramework.Models;

namespace WorkflowCore.Persistence.EntityFramework.Services
{
    public class WorkflowPurger : IWorkflowPurger
    {
        private readonly WorkflowDbContext _db;

        public WorkflowPurger(WorkflowDbContext db)
        {
            this._db = db;
        }
        
        public async Task PurgeWorkflows(WorkflowStatus status, DateTime olderThan, CancellationToken cancellationToken = default)
        {
            var olderThanUtc = olderThan.ToUniversalTime();
            
            var workflows = await _db.Set<PersistedWorkflow>().Where(x => x.Status == status && x.CompleteTime < olderThanUtc).ToListAsync(cancellationToken);
            foreach (var wf in workflows)
            {
                foreach (var pointer in wf.ExecutionPointers)
                {
                    foreach (var extAttr in pointer.ExtensionAttributes)
                    {
                        _db.Remove(extAttr);
                    }

                    _db.Remove(pointer);
                }
                _db.Remove(wf);
            }

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}