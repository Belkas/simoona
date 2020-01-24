﻿using System.Linq;
using System.Net;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Management.Scheduler;
using Microsoft.WindowsAzure.Management.Scheduler.Models;
using Microsoft.WindowsAzure.Scheduler;
using Microsoft.WindowsAzure.Scheduler.Models;
using Shrooms.Host.Contracts.Infrastructure;

namespace Shrooms.Infrastructure.CloudScheduler
{
    public static class AzureJobScheduler
    {
        private const string JobsCloudServiceName = "CS-WebJobs-NorthEurope-scheduler";
        private const string JobsCollectionName = "Simoona-EventRecurranceJobs";
        private const string JobsCollectionLabel = "Simoona-EventRecurranceJobs";
        private const string SchedulerResourceNamespace = "scheduler";

        private static readonly CertificateCloudCredentials _credentials = AzureCredentialsFactory.FromCertificateStore();
        private static readonly ILogger _logger = new Logger.Logger();

        public static void Launch<TJob>(TJob job)
            where TJob : AzureJob
        {
            CreateJobCollection();
            CreateJob(job);
        }

        private static void CreateJob<TJob>(TJob job)
            where TJob : AzureJob
        {
            if (JobAlreadyExists(job.Identifier))
            {
                return;
            }

            var schedulerClient = new SchedulerClient(JobsCloudServiceName, JobsCollectionName, _credentials);
            var jobCreationResult = schedulerClient.Jobs.CreateOrUpdate(job.Identifier, job.GenerateAzureJobParameters());

            if (jobCreationResult.StatusCode != HttpStatusCode.OK)
            {
                _logger.Error(new JobSchedulerException(jobCreationResult));
            }
        }

        private static void CreateJobCollection()
        {
            if (JobCollectionAlreadyExists())
            {
                return;
            }

            var schedulerManagementClient = new SchedulerManagementClient(_credentials);

            var eventJobCollection = new JobCollectionCreateParameters
            {
                Label = JobsCollectionLabel,
                IntrinsicSettings = new JobCollectionIntrinsicSettings
                {
                    Plan = JobCollectionPlan.Standard,
                    Quota = new JobCollectionQuota
                    {
                        MaxJobCount = 50,
                        MaxJobOccurrence = 5,
                        MaxRecurrence = new JobCollectionMaxRecurrence
                        {
                            Frequency = JobCollectionRecurrenceFrequency.Minute,
                            Interval = 5
                        }
                    }
                }
            };

            var jobCollectionCreationResult = schedulerManagementClient.JobCollections.Create(JobsCloudServiceName, JobsCollectionName, eventJobCollection);

            if (jobCollectionCreationResult.StatusCode != HttpStatusCode.OK)
            {
                _logger.Error(new JobSchedulerException(jobCollectionCreationResult));
            }
        }

        private static bool JobAlreadyExists(string jobIdentifier)
        {
            var schedulerClient = new SchedulerClient(JobsCloudServiceName, JobsCollectionName, _credentials);
            var existingJobs = schedulerClient.Jobs.List(new JobListParameters());
            var jobAlreadyExists = existingJobs.Jobs.Any(x => x.Id == jobIdentifier);
            return jobAlreadyExists;
        }

        private static bool JobCollectionAlreadyExists()
        {
            var cloudServiceManagementClient = new CloudServiceManagementClient(_credentials);
            var cloudService = cloudServiceManagementClient.CloudServices.Get(JobsCloudServiceName);
            var collectionAlreadyExists = cloudService.Resources
                .Any(x =>
                    x.Name == JobsCollectionName &&
                    x.ResourceProviderNamespace == SchedulerResourceNamespace);

            return collectionAlreadyExists;
        }
    }
}
