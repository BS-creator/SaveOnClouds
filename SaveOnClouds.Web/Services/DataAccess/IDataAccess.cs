using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SaveOnClouds.Web.Models.CloudAPI;
using SaveOnClouds.Web.Models.EnvironmentApi;
using SaveOnClouds.Web.Models.ScheduleApi;
using SaveOnClouds.Web.Models.TeamsApi;

namespace SaveOnClouds.Web.Services.DataAccess
{
    public interface IDataAccess
    {
        Task<List<string>> GetAllParentUsersIds(string myUserEmail);
        Task<List<OwnedAccountModel>> GetAllCloudAccounts(List<string> userIdList);

        Task<bool> AwsAccountExists(string awsAccountNumber);

        Task<decimal> AddAccount(CloudAccountModel model);

        Task<OwnedAccountModel> GetAccountById(long accountId);

        Task ChangeAccountName(long accountId, string newName);

        Task DeleteAccount(long accountId);

        Task<List<TeamModel>> GetTeams(string ownerId);


        Task<long> CreateTeam(CreateTeamModel model, string ownerId);

        Task<TeamModel> GetTeamByOwnerIdAndName(string ownerId, string name);

        Task<TeamModel> GetTeamById(long id);

        Task UpdateTeam(UpdateTeamModel model, string ownerId);

        Task DeleteTeam(long id, string ownerId);


        Task ChangeScheduleState(long id, bool isActive);
        Task AddScheduleDetails(ScheduleDetail details);
        Task<bool> ScheduleExists(string name, string ownerUserId);
        Task<List<Schedule>> GetAllSchedules(string id);
        Task<ScheduleDetail> GetScheduleDetails(long scheduleId);
        
        Task<Schedule> GetScheduleById(long id);
        Task<long> CreateSchedule(Schedule record);
        Task<Schedule> GetScheduleByName(string name);
        Task UpdateSchedule(Schedule record);

        Task<long> CreateInvitation(InviteUserModel model, string bossUserId, string bossEmail, string token,
            DateTime invitationDateTime);
        Task<InvitationModel> GetUserInvitationByEmail(string userEmail, string bossUserId);

        Task AcceptInvitation(string token, string email);

        Task<List<InvitationModel>> GetInvitedUsers(string bossId);

        Task DeleteInvitation(long id, string bossId);

        Task<long> AddUserToTeam(TeamUserAssociationModel model);

        Task RemoveUserFromTeam(TeamUserAssociationModel model);
        Task DeleteScheduleById(long id);

        Task<long?> GetTeamUserAssociationId(long teamId, string userId);
        Task<List<TeamUserAssociationViewModel>> GetUserTeams(string userId, string bossId);
        Task<bool> UserHasAccessToResource(long modelId, List<string> allUserIds);
        Task AssignScheduleToResource(long modelResourceId, long modelScheduleId);

        #region Environment

        Task<List<CloudEnvironment>> GetAllEnvironments(string id);
        Task<CloudEnvironment> GetEnvironmentById(long id);
        Task<CloudEnvironment> GetEnvironmentByName(string name);
        Task DeleteEnvironmentById(long id);

        /// <summary>
        /// Check environment already exists
        /// </summary>
        /// <param name="id">Current env Id (excluding from check)</param>
        /// <param name="name">Name to check</param>
        /// <param name="ownerUserId">Owner account Id</param>
        /// <returns></returns>
        Task<bool> EnvironmentExists(long id, string name, string ownerUserId);
        Task<long> CreateEnvironment(CloudEnvironment record);
        Task UpdateEnvironment(CloudEnvironment record);

        #endregion

    }
}