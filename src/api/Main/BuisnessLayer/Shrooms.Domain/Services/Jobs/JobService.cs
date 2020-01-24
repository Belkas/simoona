using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Jobs;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Host.Contracts.DAL;

namespace Shrooms.Domain.Services.Jobs
{
    public class JobService : IJobService
    {
        private readonly IDbSet<JobPosition> _jobTypesDbSet;
        private readonly IUnitOfWork2 _uow;

        public JobService(IUnitOfWork2 uow)
        {
            _uow = uow;
            _jobTypesDbSet = uow.GetDbSet<JobPosition>();
        }

        public async Task<JobTypeDTO> GetJobType(int id, UserAndOrganizationDTO userOrg)
        {
            var type = await _jobTypesDbSet
                .Where(t => t.OrganizationId == userOrg.OrganizationId && t.Id == id)
                .Select(MapJobTypesToDTO())
                .FirstOrDefaultAsync();

            if (type == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Type not found");
            }

            return type;
        }

        public async Task<IEnumerable<JobTypeDTO>> GetJobTypes(UserAndOrganizationDTO userAndOrg)
        {
            var jobTypesDTO = await _jobTypesDbSet
                .Where(t => t.OrganizationId == userAndOrg.OrganizationId)
                .Select(MapJobTypesToDTO())
                .ToListAsync();

            return jobTypesDTO;
        }

        public async Task CreateJobType(JobTypeDTO jobTypeDTO)
        {
            var alreadyExists = await _jobTypesDbSet
                .AnyAsync(t => t.Title == jobTypeDTO.Title && t.OrganizationId == jobTypeDTO.OrganizationId);

            if (alreadyExists)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Job position with that title already exists");
            }

            var newType = new JobPosition
            {
                Title = jobTypeDTO.Title,
                CreatedBy = jobTypeDTO.UserId,
                OrganizationId = jobTypeDTO.OrganizationId
            };

            _jobTypesDbSet.Add(newType);

            await _uow.SaveChangesAsync(jobTypeDTO.UserId);
        }

        public async Task RemoveJobType(int id, UserAndOrganizationDTO userOrg)
        {
            var type = await _jobTypesDbSet
                .Where(t => t.OrganizationId == userOrg.OrganizationId && t.Id == id)
                .FirstOrDefaultAsync();

            if (type == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Type not found");
            }

            _jobTypesDbSet.Remove(type);

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task UpdateJobType(JobTypeDTO jobTypeDTO)
        {
            var alreadyExists = await _jobTypesDbSet
                .AnyAsync(t => t.Title == jobTypeDTO.Title && t.OrganizationId == jobTypeDTO.OrganizationId && t.Id != jobTypeDTO.Id);

            if (alreadyExists)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Job position with that title already exists");
            }

            var type = await _jobTypesDbSet
                .Where(t => t.OrganizationId == jobTypeDTO.OrganizationId && t.Id == jobTypeDTO.Id)
                .FirstOrDefaultAsync();

            if (type == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Type not found");
            }

            type.Title = jobTypeDTO.Title;

            await _uow.SaveChangesAsync(jobTypeDTO.UserId);
        }

        private Expression<Func<JobPosition, JobTypeDTO>> MapJobTypesToDTO()
        {
            return jobType => new JobTypeDTO
            {
                Id = jobType.Id,
                Title = jobType.Title
            };
        }
    }
}
