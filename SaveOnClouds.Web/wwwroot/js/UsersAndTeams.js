var UsersAndTeams = {
    LoadUsers: function() {
        $.ajax({
            type: "get",
            contentType: "application/json",
            url: "/TeamsApi/Users",
            success: function(data) {
                UsersAndTeams.OnUsersLoaded(data);
            },
            error: function(jqXhr, textStatus, errorThrown) {
                alert("error reading users");
            }
        });
    },

    OnUsersLoaded: function(users) {
        var table = $("#nav-users").find("tbody").first();
        $.each(users,
            function(index, obj) {
                UsersAndTeams.CreateInvitationRow(table, obj);
            });

        $("button.manage-teams").off("click");
        $("button.manage-teams").on("click",
            function () {
                var btn = $(this);
                $.ajax({
                    type: "get",
                    contentType: "application/json",
                    url: "/TeamsApi/UserTeams/" + $(this).data("user-email"),
                    success: function (data) {
                        var form = $("#AssignTeamsModal").find("form").first();
                        form.children().remove();
                        var teams = [];
                        var table = $("#nav-teams").find("tbody").first();
                        var teamButtons = table.find(".edit-team");

                        var ul = $("<ul>");

                        $.each(teamButtons,
                            function (index, obj) {
                                var current = $(obj);
                                var li = $("<li>");
                                var teamId = current.data("team-id");
                                var teamName = current.data("team-name");
                                var chk = $("<input>").attr("type", "checkbox").attr("id", teamName).data("val", teamId);
                                var ind = data.findIndex(x => x.teamId === teamId);
                                if (ind > -1) {
                                    //chk.attr("checked", "checked");
                                    chk.prop("checked", true);
                                }
                                li.append(chk);
                                var lbl = $("<label>").attr("for", teamName).html(teamName);
                                li.append(lbl);
                                ul.append(li);
                            });
                        form.append(ul);
                        $("#AssignTeamsModal").modal("show", btn);
                    },
                    error: function(jqXhr, textStatus, errorThrown) {
                        alert("error reading user teams");
                    }
                });
            });

        $("button.remove-user").off("click");
        $("button.remove-user").on("click",
            function() {
                if (confirm("Are you sure?")) {
                    UsersAndTeams.RemoveInvitation($(this).data("inv-id"));
                }
            });
    },

    CreateInvitationRow: function(table, obj) {
        var tr = $("<tr>");

        tr.append($("<td>").html(obj.userEmail));

        var dateUtc = new Date(obj.inviteDateTimeUtc + " UTC");
        var strDateLocal = dateUtc.toString();
        var dateLocal = new Date(strDateLocal).toLocaleString();
        var tdInvitation = $("<td>").html(dateLocal);
        tr.append(tdInvitation);

        tr.append($("<td>").html(obj.accepted ? "Active" : "Pending Invitation"));

        var tdActions = $("<td>");
        if (obj.accepted) {
            tdActions.append($("<button>").attr("type", "button").addClass("btn").addClass("btn-primary").addClass("btn-sm").addClass("manage-teams").html("Manage Teams").data("user-email", obj.userEmail));
        }

        tdActions.append($("<button>").attr("type", "button").addClass("btn").addClass("btn-primary").addClass("btn-sm").addClass("remove-user").html("Remove").data("inv-id", obj.id));
        tr.append(tdActions);

        table.append(tr);
    },

    RemoveInvitation: function(id) {
        $.ajax({
            type: "delete",
            contentType: "application/json",
            url: "/TeamsApi/RemoveInvitation/" + id,
            success: function (response) {
                var table = $("#nav-users").find("tbody").first();
                table.find(".remove-user").each(function (index, obj) {
                    var current = $(obj);
                    if (current.data("team-id") === id) {
                        var tr = current.closest("tr");
                        tr.fadeOut("slow", function () {
                            tr.remove();
                        });
                    }
                });
            },
            error: function (jqXhr, textStatus, errorThrown) {
                alert(jqXhr.responseJSON.error);
            }
        });
    },

    LoadTeams: function () {
        $.ajax({
            type: "get",
            contentType: "application/json",
            url: "/TeamsApi",
            success: function (data) {
                UsersAndTeams.OnTeamsLoaded(data);
            },
            error: function (jqXhr, textStatus, errorThrown) {
                alert("error reading teams");
            }
        });
    },

    OnTeamsLoaded: function (teams) {
        var table = $("#nav-teams").find("tbody").first();
        $.each(teams,
            function (index, obj) {
                UsersAndTeams.CreateTeamRow(table, obj.name, obj.id);
            });
        UsersAndTeams.OnTeamRowCreated();
    },

    CreateTeamRow: function (table, name, id) {
        var tr = $("<tr>");
        tr.append($("<td>").html(name));

        var tdActions = $("<td>");
        tdActions.append($("<button>").attr("type", "button").addClass("btn").addClass("btn-primary").addClass("btn-sm").addClass("edit-team").html("Edit").data("team-id", id).data("team-name", name));
        tdActions.append($("<button>").attr("type", "button").addClass("btn").addClass("btn-primary").addClass("btn-sm").addClass("remove-team").html("Remove").data("team-id", id).data("team-name", name));

        tr.append(tdActions);

        table.append(tr);
    },

    Bind: function () {
        $("#InviteUsersModal").on("show.bs.modal", UsersAndTeams.OnInviteUserModalOpened);
        $("#InviteUsersModal").on("hidden.bs.modal", UsersAndTeams.OnInviteUserModalClosed);

        $("#CreateTeamModal").on("show.bs.modal", UsersAndTeams.OnCreateTeamModalOpened);
        $("#CreateTeamModal").on("hidden.bs.modal", UsersAndTeams.OnCreateTeamModalClosed);

        $("#EditTeamModal").on("show.bs.modal", UsersAndTeams.OnEditTeamModalOpened);

        $("#AssignTeamsModal").on("show.bs.modal", UsersAndTeams.OnAssignTeamsModalOpened);
    },

    OnAssignTeamsModalOpened: function (e) {
        var btn = $(e.relatedTarget);
        $("#BtnUserTeamsAssign").data("user-email", btn.data("user-email"));
        $("#BtnUserTeamsAssign").off("click");
        $("#BtnUserTeamsAssign").on("click",
            function() {
                var data = {
                    Email: $(this).data("user-email"),
                    Teams: []
                };

                $("#AssignTeamsModal input[type='checkbox']").each(function(index, obj) {
                    var el = $(obj);
                    var isChecked = el.prop("checked");

                    var team = {
                        TeamId: el.data("val"),
                        Assigned: isChecked
                    };
                    data.Teams.push(team);
                });

                $.ajax({
                    type: "post",
                    contentType: "application/json",
                    url: "/TeamsApi/AssignTeams",
                    data: JSON.stringify(data),
                    success: function(response) {
                        $("#AssignTeamsModal").modal("hide");
                    },
                    error: function(jqXhr, textStatus, errorThrown) {
                        alert(jqXhr.responseJSON.error);
                    }
                });
            });
    },

    OnInviteUserModalClosed: function () {
        $("#InviteUsersModal").find(".form-group").not(":first").remove();
        $("#InvitedUser0").removeClass("error").val("");
        $("#InviteUsersModal").find("label.error").remove();
    },

    OnInviteUserModalOpened: function () {
        $("#BtnAddAnotherUser").off("click");
        $("#BtnAddAnotherUser").on("click", UsersAndTeams.AddAnotherUser);

        $("#BtnInviteUsersSave").off("click");
        $("#BtnInviteUsersSave").on("click", UsersAndTeams.InviteUsers);
    },

    InviteUsers: function () {
        var form = $("#InviteUsersModal").find("form").first();
        if (form.valid()) {
            var data = [];

            $("input[id^='InvitedUser']").each(function(index, obj) {
                var email = {
                    EmailAddress: $(obj).val()
                };
                data.push(email);
            });

            $.ajax({
                type: "post",
                contentType: "application/json",
                url: "/TeamsApi/Invite",
                data: JSON.stringify(data),
                success: function(response) {
                    UsersAndTeams.OnUsersLoaded(response);
                    $("#InviteUsersModal").modal("hide");
                },
                error: function(jqXhr, textStatus, errorThrown) {
                    alert(jqXhr.responseJSON.error);
                }
            });
        }
    },

    AddAnotherUser: function () {
        var dialog = $("#InviteUsersModal");
        var lastFormGroup = dialog.find(".form-group").last();
        var tmpId = parseInt(lastFormGroup.find("input").first().attr("id").substring(11), 10) + 1;
        var form = dialog.find("form");
        var newId = "InvitedUser" + tmpId;

        var lbl = $("<label>").attr("for", newId).addClass("col-sm-3").addClass("col-form-label").html("Email:");
        var txt = $("<input>").attr("type", "text").attr("id", newId).addClass("form-control").attr("name", newId).data("rule-required", true).data("msg-required", "Email is required to invite user").data("rule-email", true).data("msg-email", "Email address must be in the format of name@domain.com");
        var txtDiv = $("<div>").addClass("col-sm-7").append(txt);
        var btn = $("<button>").attr("type", "button").addClass("btn").addClass("btn-danger").addClass("btn-sm")
            .html("X");
        var btnDiv = $("<div>").addClass("col-sm-2").append(btn);

        var formGroup = $("<div>").addClass("form-group").addClass("row");
        formGroup.append(lbl).append(txtDiv).append(btnDiv);
        form.append(formGroup);

        var removeButtons = form.find(".btn");
        removeButtons.off("click");
        removeButtons.on("click",
            function () {
                var fg = $(this).closest(".form-group");
                fg.fadeOut("slow",
                    function () {
                        fg.remove();
                    });
            });
    },

    OnCreateTeamModalOpened: function () {
        $("#BtnCreateTeamSave").off("click");
        $("#BtnCreateTeamSave").on("click", UsersAndTeams.CreateTeam);
    },

    OnCreateTeamModalClosed: function () {
        $("#CreateTeamModal").find(".form-group").not(":first").remove();
        $("#NewTeamName").removeClass("error").val("");
    },

    CreateTeam: function () {
        var form = $("#CreateTeamModal").find("form").first();

        if (form.valid()) {
            var data = {
                Name: $("#NewTeamName").val()
            };

            $.ajax({
                type: "post",
                contentType: "application/json",
                url: "/TeamsApi",
                data: JSON.stringify(data),
                success: function (response) {
                    var table = $("#nav-teams").find("tbody").first();
                    UsersAndTeams.CreateTeamRow(table, data.Name, response.teamId);
                    UsersAndTeams.OnTeamRowCreated();
                    $("#CreateTeamModal").modal("hide");
                },
                error: function (jqXhr, textStatus, errorThrown) {
                    alert(jqXhr.responseJSON.error);
                }
            });
        }
    },

    OnTeamRowCreated: function() {
        $("button.edit-team").off("click");
        $("button.edit-team").on("click",
            function() {
                $("#EditTeamModal").modal("show", $(this));
            });

        $("button.remove-team").off("click");
        $("button.remove-team").on("click",
            function() {
                if (confirm("Are you sure?")) {
                    UsersAndTeams.DeleteTeam($(this).data("team-id"));
                }
            });
    },

    OnEditTeamModalOpened: function(e) {
        var btn = $(e.relatedTarget);
        $("#EditedTeamName").val(btn.data("team-name")).data("team-id", btn.data("team-id"));

        $("#BtnEditTeamSave").off("click");
        $("#BtnEditTeamSave").on("click", UsersAndTeams.EditTeam);
    },

    EditTeam: function() {
        var form = $("#CreateTeamModal").find("form").first();

        if (form.valid()) {
            var data = {
                Id: $("#EditedTeamName").data("team-id"),
                Name: $("#EditedTeamName").val()
            };

            $.ajax({
                type: "put",
                contentType: "application/json",
                url: "/TeamsApi",
                data: JSON.stringify(data),
                success: function (response) {
                    var table = $("#nav-teams").find("tbody").first();
                    table.find(".edit-team").each(function(index, obj) {
                        var current = $(obj);
                        if (current.data("team-id") === data.Id) {
                            current.data("team-name", data.Name);
                            current.closest("tr").find("td").first().html(data.Name);
                        }
                    });
                    $("#EditTeamModal").modal("hide");
                },
                error: function (jqXhr, textStatus, errorThrown) {
                    alert(jqXhr.responseJSON.error);
                }
            });
        }
    },

    DeleteTeam: function(id) {
        $.ajax({
            type: "delete",
            contentType: "application/json",
            url: "/TeamsApi/" + id,
            success: function (response) {
                var table = $("#nav-teams").find("tbody").first();
                table.find(".remove-team").each(function (index, obj) {
                    var current = $(obj);
                    if (current.data("team-id") === id) {
                        var tr = current.closest("tr");
                        tr.fadeOut("slow", function() {
                            tr.remove();
                        });
                    }
                });
            },
            error: function (jqXhr, textStatus, errorThrown) {
                alert(jqXhr.responseJSON.error);
            }
        });
    }
};