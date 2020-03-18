using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SaveOnClouds.CloudFuncs.Common.Models;
using SaveOnClouds.Web.Models.CloudAPI;
using SaveOnClouds.Web.Models.EnvironmentApi;
using SaveOnClouds.Web.Models.ScheduleApi;
using SaveOnClouds.Web.Models.TeamsApi;
using DayOfWeek = SaveOnClouds.Web.Models.ScheduleApi.DayOfWeek;

namespace SaveOnClouds.Web.Services.DataAccess
{
    public class AdoDataAccess : IDataAccess, IDisposable
    {
        private readonly SqlConnection _connection;

        public AdoDataAccess(IConfiguration configuration)
        {
            var connectionString = configuration["ConnectionStrings:Default"];
            _connection = new SqlConnection(connectionString);
        }

        public async Task<List<string>> GetAllParentUsersIds(string myUserEmail)
        {
            if (string.IsNullOrEmpty(myUserEmail))
                throw new InvalidEnumArgumentException(
                    "Parameter myUserEmail cannot be empty, in method GetAllParentUsersIds");

            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "GetParentUsers";
            command.Parameters.AddWithValue("@UserEmail", myUserEmail);
            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();

            var result = new List<string>();

            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            while (await reader.ReadAsync())
            {
                var bossUserId = await reader.GetFieldValueAsync<string>("BossUserId");
                result.Add(bossUserId);
            }

            return result;
        }

        public async Task<List<OwnedAccountModel>> GetAllCloudAccounts(List<string> userIdList)
        {
            const string queryTemplate =
                "Select Id, AccountName, AccountType, CreatorUserID from CloudAccounts where CreatorUserID in ({0})";
            var queryParameter = string.Join(',', userIdList.Select(x => $"'{x}'"));
            var query = string.Format(queryTemplate, queryParameter);

            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();

            var result = new List<OwnedAccountModel>();
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);

            while (await reader.ReadAsync())
            {
                var record = new OwnedAccountModel
                {
                    Id = await reader.GetFieldValueAsync<long>("Id"),
                    AccountName = await reader.GetFieldValueAsync<string>("AccountName"),
                    AccountType = await reader.GetFieldValueAsync<int>("AccountType"),
                    CreatorUserId = await reader.GetFieldValueAsync<string>("CreatorUserID")
                };
                result.Add(record);
            }

            return result;
        }

        public async Task<bool> AwsAccountExists(string awsAccountNumber)
        {
            if (string.IsNullOrEmpty(awsAccountNumber))
                throw new InvalidEnumArgumentException(
                    "Parameter awsAccountNumber cannot be empty, in method AwsAccountExists");

            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "AwsAccountExists";
            command.Parameters.AddWithValue("@AwsAccountNumber", awsAccountNumber);
            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();

            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            return reader.HasRows;
        }

        public async Task<decimal> AddAccount(CloudAccountModel model)
        {
            if (model.AccountType == CloudAccountType.Aws)
            {
                var accountExists = await AwsAccountExists(model.AwsAccountNumber);
                if (accountExists)
                    throw new ArgumentException("An AWS Account with this AWS Account Number already exists.");
            }

            if (model.AccountType == CloudAccountType.GoogleCloud)
            {
                var keyObject = JsonConvert.DeserializeObject<GoogleCloudKey>(model.GcJsonBody);
                var accountExists = await GoogleCloudAccountExists(keyObject.ClientId);
                if (accountExists)
                    throw new ArgumentException("This Client Id is already registered.");
            }

            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "CreateAccount";
            command.Parameters.AddWithValue("@CreatorUserID", model.CreatorUserId);
            command.Parameters.AddWithValue("@AccountType", model.AccountType);
            command.Parameters.AddWithValue("@AWSRoleArn", model.AwsRoleArn);
            command.Parameters.AddWithValue("@AWSAccountNumber", model.AwsAccountNumber);
            command.Parameters.AddWithValue("@AccountName", model.AccountName);
            command.Parameters.AddWithValue("@AWSRegionName", model.AwsRegionName);
            command.Parameters.AddWithValue("@SourceAccountNumber", model.SourceAccountNumber);
            command.Parameters.AddWithValue("@ExternalID", model.AwsExternalId);

            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            var reader = await command.ExecuteReaderAsync();
            await reader.ReadAsync();
            return reader.GetDecimal(0);
        }

        public async Task<OwnedAccountModel> GetAccountById(long accountId)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "GetAccountById";
            command.Parameters.AddWithValue("@Id", accountId);

            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);

            if (await reader.ReadAsync())
            {
                var record = new OwnedAccountModel
                {
                    Id = await reader.GetFieldValueAsync<long>("Id"),
                    AccountName = await reader.GetFieldValueAsync<string>("AccountName"),
                    AccountType = await reader.GetFieldValueAsync<int>("AccountType"),
                    CreatorUserId = await reader.GetFieldValueAsync<string>("CreatorUserID")
                };
                return record;
            }

            return null;
        }

        public async Task ChangeAccountName(long accountId, string newName)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "ChangeCloudAccountName";
            command.Parameters.AddWithValue("@Id", accountId);
            command.Parameters.AddWithValue("@Name", newName);
            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteAccount(long accountId)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "DeleteCloudAccount";
            command.Parameters.AddWithValue("@Id", accountId);
            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        void IDisposable.Dispose()
        {
            _connection?.Dispose();
        }

        private async Task<bool> GoogleCloudAccountExists(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new InvalidEnumArgumentException(
                    "Parameter Client Id cannot be empty, in method GoogleCloudAccountExists");

            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "GoogleCloudAccountExists";
            command.Parameters.AddWithValue("@clientId", clientId);
            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();

            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            return reader.HasRows;
        }


        #region Schedules

        public async Task<Schedule> GetScheduleById(long id)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "GetScheduleById";
            command.Parameters.AddWithValue("@Id", id);
            if (_connection.State == ConnectionState.Closed) await _connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            if (await reader.ReadAsync())
            {
                var schedule = new Schedule
                {
                    Id = await reader.GetFieldValueAsync<long>("Id"),
                    Name = await reader.GetFieldValueAsync<string>("Name"),
                    Description = await reader.GetFieldValueAsync<string>("Description"),
                    TimeZoneName = await reader.GetFieldValueAsync<string>("TimeZoneName"),
                    IsActive = await reader.GetFieldValueAsync<bool>("IsActive")
                };

                return schedule;
            }

            return null;
        }

        public async Task<List<Schedule>> GetAllSchedules(string id)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "GetAllSchedules";
            command.Parameters.AddWithValue("@OwnerAccountId", id);
            if (_connection.State == ConnectionState.Closed) await _connection.OpenAsync();
            var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            var schedules = new List<Schedule>();
            while (await reader.ReadAsync())
            {
                var schedule = new Schedule
                {
                    Id = await reader.GetFieldValueAsync<long>("Id"),
                    Name = await reader.GetFieldValueAsync<string>("Name"),
                    Description = await reader.GetFieldValueAsync<string>("Description"),
                    TimeZoneName = await reader.GetFieldValueAsync<string>("TimeZoneName"),
                    IsActive = await reader.GetFieldValueAsync<bool>("IsActive")
                };
                schedules.Add(schedule);
            }

            return schedules;
        }

        public async Task<long> CreateSchedule(Schedule record)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "CreateSchedule";
            command.Parameters.AddWithValue("@Name", record.Name);
            command.Parameters.AddWithValue("@Description", record.Description);
            command.Parameters.AddWithValue("@TimeZoneName", record.TimeZoneName);
            command.Parameters.AddWithValue("@OwnerAccountId", record.OwnerUserId);
            command.Parameters.AddWithValue("@IsActive", 1);
            if (_connection.State == ConnectionState.Closed) await _connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();
            await reader.ReadAsync();
            return (long) reader.GetDecimal(0);
        }

        public async Task<Schedule> GetScheduleByName(string name)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "GetScheduleByName";
            command.Parameters.AddWithValue("@Name", name);
            if (_connection.State == ConnectionState.Closed) await _connection.OpenAsync();
            var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            if (await reader.ReadAsync())
                return new Schedule
                {
                    Id = (long) await reader.GetFieldValueAsync<decimal>("Id"),
                    Name = await reader.GetFieldValueAsync<string>("Name"),
                    Description = await reader.GetFieldValueAsync<string>("Description"),
                    TimeZoneName = await reader.GetFieldValueAsync<string>("TimeZoneName"),
                    IsActive = await reader.GetFieldValueAsync<bool>("IsActive"),
                    OwnerUserId = await reader.GetFieldValueAsync<string>("OwnerAccountId")
                };

            return null;
        }


        public async Task<bool> ScheduleExists(string name, string ownerUserId)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "GetIsDuplicateName";
            command.Parameters.AddWithValue("@Name", name);
            command.Parameters.AddWithValue("@UserId", ownerUserId);
            if (_connection.State == ConnectionState.Closed) await _connection.OpenAsync();
            var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);

            return reader.HasRows;
        }


        public async Task UpdateSchedule(Schedule record)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "UpdateSchedule";
            command.Parameters.AddWithValue("@Name", record.Name);
            command.Parameters.AddWithValue("@Description", record.Description);
            command.Parameters.AddWithValue("@TimeZoneName", record.TimeZoneName);
            command.Parameters.AddWithValue("@Id", record.Id);
            command.Parameters.AddWithValue("@IsActive", record.IsActive ? 1 : 0);
            if (_connection.State == ConnectionState.Closed) await _connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task ChangeScheduleState(long id, bool isActive)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "ChangeScheduleState";
            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);
            if (_connection.State == ConnectionState.Closed) await _connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }


        public async Task<ScheduleDetail> GetScheduleDetails(long scheduleId)
        {
            const string queryTemplate =
                "Select * from ScheduleDetails where ScheduleId = {0}";
            var query = string.Format(queryTemplate, scheduleId);

            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);

            var result = new ScheduleDetail
            {
                DayOfWeeks = new List<DayOfWeek>()
            };

            while (await reader.ReadAsync())
            {
                result.ScheduleId = await reader.GetFieldValueAsync<long>("ScheduleId");

                var dayOfWeek = new DayOfWeek
                {
                    DayIndex = await reader.GetFieldValueAsync<int>("DayOfTheWeek"),
                    ScheduleId = await reader.GetFieldValueAsync<long>("ScheduleId"),
                    Hours = new List<Hour>
                    {
                        new Hour
                        {
                            HourIndex = await reader.GetFieldValueAsync<int>("HourOfDay"),
                            Quarter1 = await reader.GetFieldValueAsync<bool>("Quarter1"),
                            Quarter2 = await reader.GetFieldValueAsync<bool>("Quarter2"),
                            Quarter3 = await reader.GetFieldValueAsync<bool>("Quarter3"),
                            Quarter4 = await reader.GetFieldValueAsync<bool>("Quarter4")
                        }
                    }
                };

                result.DayOfWeeks.Add(dayOfWeek);
            }

            return result;
        }


        public async Task AddScheduleDetails(ScheduleDetail details)
        {
            const string insertTemplate =
                "Insert into ScheduleDetails (ScheduleId, DayOfTheWeek, HourOfDay, Quarter1, Quarter2, Quarter3, Quarter4) Values({0}, {1}, {2}, {3}, {4}, {5}, {6})";

            var assembly = Assembly.GetExecutingAssembly();
            using var reader =
                new StreamReader(
                    assembly.GetManifestResourceStream(
                        "SaveOnClouds.Web.EmbededObjects.ScheduleDetailsQuery.txt") ??
                    throw new InvalidOperationException("SQL Template Cannot Be Found!"));
            var sqlTemplateBody = await reader.ReadToEndAsync();

            var builder = new StringBuilder();
            foreach (var dayOfWeek in details.DayOfWeeks)
            foreach (var hour in dayOfWeek.Hours)
                builder.AppendLine(string.Format(insertTemplate, details.ScheduleId, dayOfWeek.DayIndex, hour.HourIndex,
                    hour.Quarter1 ? 1 : 0, hour.Quarter2 ? 1 : 0, hour.Quarter3 ? 1 : 0, hour.Quarter4 ? 1 : 0));

            var insertStatement = builder.ToString();
            var finalSql = string.Format(sqlTemplateBody, details.ScheduleId, insertStatement);

            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = finalSql;
            if (_connection.State == ConnectionState.Closed) await _connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteScheduleById(long id)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "DeleteScheduleById";
            command.Parameters.AddWithValue("@Id", id);
            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        #endregion

        #region Teams and User Invitation

        public async Task<long> AddUserToTeam(TeamUserAssociationModel model)
        {
            var id = await GetTeamUserAssociationId(model.TeamId, model.UserId);
            if (id.HasValue) throw new ArgumentException("User is already member of team");

            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "AddUserToTeam";
            command.Parameters.AddWithValue("@TeamId", model.TeamId);
            command.Parameters.AddWithValue("@UserId", model.UserId);

            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            await reader.ReadAsync();
            return (long) reader.GetDecimal(0);
        }


        public async Task RemoveUserFromTeam(TeamUserAssociationModel model)
        {
            var id = await GetTeamUserAssociationId(model.TeamId, model.UserId);
            if (!id.HasValue) throw new ArgumentException("User is not member of team");

            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "RemoveUserFromTeam";
            command.Parameters.AddWithValue("@Id", id);

            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task<long?> GetTeamUserAssociationId(long teamId, string userId)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "GetTeamUserAssociation";
            command.Parameters.AddWithValue("@TeamId", teamId);
            command.Parameters.AddWithValue("@UserId", userId);

            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);

            if (await reader.ReadAsync()) return await reader.GetFieldValueAsync<long>("Id");

            return null;
        }

        public async Task<List<TeamUserAssociationViewModel>> GetUserTeams(string userId, string bossId)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "GetUserTeamsByUserId";
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@BossId", bossId);

            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);

            var response = new List<TeamUserAssociationViewModel>();

            while (await reader.ReadAsync())
                response.Add(new TeamUserAssociationViewModel
                {
                    Id = await reader.GetFieldValueAsync<long>("Id"),
                    TeamId = await reader.GetFieldValueAsync<long>("TeamId"),
                    UserId = await reader.GetFieldValueAsync<string>("UserId")
                });

            return response;
        }


        public async Task<bool> UserHasAccessToResource(long resourceId, List<string> allUserIds)
        {
            var queryTemplate =
                $"Select 1 as C from CloudAccounts A inner join CloudResources C on A.Id = C.CloudAccountId where A.CreatorUserID in ('{0}') ";
            var joinedIds = string.Join(',', allUserIds.Select(x => $"'{x}'"));
            var query = string.Format(queryTemplate, joinedIds);
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            return await reader.ReadAsync();
        }

        public async Task AssignScheduleToResource(long modelResourceId, long modelScheduleId)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "AssignScheduleToResource";
            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }


        public async Task<long> CreateTeam(CreateTeamModel model, string ownerId)
        {
            var existingTeam = await GetTeamByOwnerIdAndName(ownerId, model.Name);
            if (existingTeam != null) throw new ArgumentException($"A Team with name '{model.Name}' already exists.");

            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "CreateTeam";
            command.Parameters.AddWithValue("@OwnerId", ownerId);
            command.Parameters.AddWithValue("@Name", model.Name);

            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            await reader.ReadAsync();
            return (long) reader.GetDecimal(0);
        }

        public async Task<TeamModel> GetTeamByOwnerIdAndName(string ownerId, string name)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "GetTeamByOwnerIdAndName";
            command.Parameters.AddWithValue("@OwnerId", ownerId);
            command.Parameters.AddWithValue("@Name", name);

            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);

            if (await reader.ReadAsync())
                return new TeamModel
                {
                    Id = await reader.GetFieldValueAsync<long>("Id"),
                    Name = await reader.GetFieldValueAsync<string>("Name"),
                    OwnerId = await reader.GetFieldValueAsync<string>("OwnerId")
                };

            return null;
        }


        public async Task<TeamModel> GetTeamById(long id)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "GetTeamById";
            command.Parameters.AddWithValue("@Id", id);

            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);

            if (await reader.ReadAsync())
                return new TeamModel
                {
                    Id = await reader.GetFieldValueAsync<long>("Id"),
                    Name = await reader.GetFieldValueAsync<string>("Name"),
                    OwnerId = await reader.GetFieldValueAsync<string>("OwnerId")
                };

            return null;
        }

        public async Task UpdateTeam(UpdateTeamModel model, string ownerId)
        {
            var existingTeam = await GetTeamById(model.Id);
            if (existingTeam == null)
                throw new ArgumentException($"A team with id: {model.Id} does not exist anymore.");

            existingTeam = await GetTeamByOwnerIdAndName(ownerId, model.Name);
            if (existingTeam != null) throw new ArgumentException($"A Team with name '{model.Name}' already exists.");

            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "UpdateTeam";
            command.Parameters.AddWithValue("@Id", model.Id);
            command.Parameters.AddWithValue("@Name", model.Name);

            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteTeam(long id, string ownerId)
        {
            var existingTeam = await GetTeamById(id);

            if (existingTeam == null) throw new ArgumentException($"A team with id: {id} does not exist anymore.");

            if (existingTeam.OwnerId != ownerId)
                throw new ArgumentException("You are not owner of selected team. You have no permission to delete it.");

            var numOfTeamMembers = await GetNumberOfTeamMembers(id);
            if (numOfTeamMembers > 0)
                throw new ArgumentException("Selected team has members. Remove members from team to delete it.");

            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "DeleteTeam";
            command.Parameters.AddWithValue("@Id", id);

            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        private async Task<int> GetNumberOfTeamMembers(long id)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "GetNumberOfTeamMembers";
            command.Parameters.AddWithValue("@Id", id);

            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            return (int) await command.ExecuteScalarAsync();
        }


        public async Task<List<TeamModel>> GetTeams(string ownerId)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "GetTeamByOwnerId";
            command.Parameters.AddWithValue("@OwnerId", ownerId);

            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);

            var response = new List<TeamModel>();

            while (await reader.ReadAsync())
                response.Add(new TeamModel
                {
                    Id = await reader.GetFieldValueAsync<long>("Id"),
                    Name = await reader.GetFieldValueAsync<string>("Name"),
                    OwnerId = await reader.GetFieldValueAsync<string>("OwnerId")
                });

            return response;
        }


        public async Task<long> CreateInvitation(InviteUserModel model, string bossUserId, string bossEmail,
            string token, DateTime invitationDateTime)
        {
            var existingInvitation = await GetUserInvitationByEmail(model.EmailAddress.ToLower(), bossUserId);
            if (existingInvitation != null)
                throw new ArgumentException($"An invitation for email: {model.EmailAddress.ToLower()} already exists.");

            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "CreateInvitation";
            command.Parameters.AddWithValue("@BossUserId", bossUserId);
            command.Parameters.AddWithValue("@BossEmail", bossEmail);
            command.Parameters.AddWithValue("@UserEmail", model.EmailAddress.ToLower());
            command.Parameters.AddWithValue("@Token", token);
            command.Parameters.AddWithValue("@InvitationDate", invitationDateTime);

            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            await reader.ReadAsync();
            return (long) reader.GetDecimal(0);
        }

        public async Task<InvitationModel> GetUserInvitationByEmail(string userEmail, string bossUserId)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "GetInvitationByEmail";
            command.Parameters.AddWithValue("@UserEmail", userEmail);
            command.Parameters.AddWithValue("@BossUserId", bossUserId);

            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);

            if (await reader.ReadAsync())
                return new InvitationModel
                {
                    Id = await reader.GetFieldValueAsync<long>("Id"),
                    BossUserId = await reader.GetFieldValueAsync<string>("BossUserId"),
                    BossEmail = await reader.GetFieldValueAsync<string>("BossEmail"),
                    UserEmail = await reader.GetFieldValueAsync<string>("UserEmail"),
                    Token = await reader.GetFieldValueAsync<string>("Token"),
                    Accepted = await reader.GetFieldValueAsync<bool>("Accepted"),
                    InviteDateTimeUtc = await reader.GetFieldValueAsync<DateTime>("InviteDateTimeUTC")
                    //AcceptedDateTimeUtc = await reader.GetFieldValueAsync<DateTime?>("AcceptedDateTimeUTC")
                };

            return null;
        }

        private async Task<InvitationModel> GetUserInvitationByToken(string token)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "GetInvitationByToken";
            command.Parameters.AddWithValue("@Token", token);

            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);

            if (await reader.ReadAsync())
                return new InvitationModel
                {
                    Id = await reader.GetFieldValueAsync<long>("Id"),
                    BossUserId = await reader.GetFieldValueAsync<string>("BossUserId"),
                    BossEmail = await reader.GetFieldValueAsync<string>("BossEmail"),
                    UserEmail = await reader.GetFieldValueAsync<string>("UserEmail"),
                    Token = await reader.GetFieldValueAsync<string>("Token"),
                    Accepted = await reader.GetFieldValueAsync<bool>("Accepted"),
                    InviteDateTimeUtc = await reader.GetFieldValueAsync<DateTime>("InviteDateTimeUTC")
                    //AcceptedDateTimeUtc = await reader.GetFieldValueAsync<DateTime?>("AcceptedDateTimeUTC")
                };

            return null;
        }

        public async Task AcceptInvitation(string token, string email)
        {
            var invitation = await GetUserInvitationByToken(token);
            if (invitation == null) throw new ArgumentException("Invitation does not exist.");

            if (invitation.UserEmail != email) throw new ArgumentException("Wrong invitation");

            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "AcceptInvitation";
            command.Parameters.AddWithValue("@Id", invitation.Id);

            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task<List<InvitationModel>> GetInvitedUsers(string bossId)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "GetInvitationByBossId";
            command.Parameters.AddWithValue("@BossUserId", bossId);

            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);

            var response = new List<InvitationModel>();

            while (await reader.ReadAsync())
                response.Add(new InvitationModel
                {
                    Id = await reader.GetFieldValueAsync<long>("Id"),
                    BossUserId = await reader.GetFieldValueAsync<string>("BossUserId"),
                    BossEmail = await reader.GetFieldValueAsync<string>("BossEmail"),
                    UserEmail = await reader.GetFieldValueAsync<string>("UserEmail"),
                    Token = await reader.GetFieldValueAsync<string>("Token"),
                    Accepted = await reader.GetFieldValueAsync<bool>("Accepted"),
                    InviteDateTimeUtc = await reader.GetFieldValueAsync<DateTime>("InviteDateTimeUTC")
                    //AcceptedDateTimeUtc = await reader.GetFieldValueAsync<DateTime?>("AcceptedDateTimeUTC")
                });

            return response;
        }


        public async Task DeleteInvitation(long id, string bossId)
        {
            var existingInvitation = await GetUserInvitationById(id);
            if (existingInvitation == null)
                throw new ArgumentException($"A user invitation with id: {id} does not exist anymore.");

            if (existingInvitation.BossUserId != bossId)
                throw new ArgumentException(
                    "You are not owner of selected invitation. You have no permission to delete it.");

            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "DeleteTeam";
            command.Parameters.AddWithValue("@Id", id);

            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }


        private async Task<InvitationModel> GetUserInvitationById(long id)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "GetUserInvitationById";
            command.Parameters.AddWithValue("@Id", id);

            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);

            if (await reader.ReadAsync())
                return new InvitationModel
                {
                    Id = await reader.GetFieldValueAsync<long>("Id"),
                    BossUserId = await reader.GetFieldValueAsync<string>("BossUserId"),
                    BossEmail = await reader.GetFieldValueAsync<string>("BossEmail"),
                    UserEmail = await reader.GetFieldValueAsync<string>("UserEmail"),
                    Token = await reader.GetFieldValueAsync<string>("Token"),
                    Accepted = await reader.GetFieldValueAsync<bool>("Accepted"),
                    InviteDateTimeUtc = await reader.GetFieldValueAsync<DateTime>("InviteDateTimeUTC")
                    //AcceptedDateTimeUtc = await reader.GetFieldValueAsync<DateTime?>("AcceptedDateTimeUTC")
                };

            return null;
        }

        #endregion

        #region Environment

        public async Task<List<CloudEnvironment>> GetAllEnvironments(string id)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "GetAllEnvironments";
            command.Parameters.AddWithValue("@OwnerAccountId", id);
            if (_connection.State == ConnectionState.Closed) await _connection.OpenAsync();
            var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            var environments = new List<CloudEnvironment>();
            while (await reader.ReadAsync())
            {
                var environment = await reader.MapToEnvironment();
                environments.Add(environment);
            }

            return environments;
        }

        public async Task<CloudEnvironment> GetEnvironmentById(long id)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "GetEnvironmentById";
            command.Parameters.AddWithValue("@Id", id);
            if (_connection.State == ConnectionState.Closed) await _connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            if (await reader.ReadAsync())
            {
                var environment = await reader.MapToEnvironment();

                return environment;
            }

            return null;
        }

        public async Task<CloudEnvironment> GetEnvironmentByName(string name)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "GetEnvironmentByName";
            command.Parameters.AddWithValue("@Name", name);
            if (_connection.State == ConnectionState.Closed) await _connection.OpenAsync();
            var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            if (await reader.ReadAsync())
                return await reader.MapToEnvironment();

            return null;
        }

        public async Task DeleteEnvironmentById(long id)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "DeleteEnvironmentById";
            command.Parameters.AddWithValue("@Id", id);
            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task<bool> EnvironmentExists(long id, string name, string ownerUserId)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "EnvironmentExists";
            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@Name", name);
            command.Parameters.AddWithValue("@OwnerAccountId", ownerUserId);
            if (_connection.State == ConnectionState.Closed) await _connection.OpenAsync();
            var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);

            return reader.HasRows;
        }

        public async Task<long> CreateEnvironment(CloudEnvironment record)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "CreateEnvironment";
            command.Parameters.AddEnvironmentValues(record);
            if (_connection.State == ConnectionState.Closed) await _connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();
            await reader.ReadAsync();
            return (long) reader.GetDecimal(0);
        }

        public async Task UpdateEnvironment(CloudEnvironment record)
        {
            await using var command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "UpdateEnvironment";
            command.Parameters.AddWithValue("@Id", record.Id);
            command.Parameters.AddEnvironmentValues(record);
            if (_connection.State == ConnectionState.Closed) await _connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        #endregion
    }
}