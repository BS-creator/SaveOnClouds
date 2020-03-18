var GoogleCloud = {
    Bind: function () {

        $("#btnTestConnection").click = function () {
            var projectId = $("#ProjectId").val();
            var keyJson = $("#KeyJson").val();
            var userId = $("#UserId").val();
            var name = $("#Name").val();
            GoogleCloud.TestConnection(projectId, keyJson, userId, name);
        };

        $("#btnCreate").bind("click",
            function () {
                var projectId = $("#ProjectId").val();
                var keyJson = $("#KeyJson").val();
                var userId = $("#UserId").val();
                var name = $("#Name").val();
                GoogleCloud.CreateAccount(projectId, keyJson, userId, name);
            });
    },
    CreateAccount: function (projectId, keyJson, userId, name) {
        var apiUrl = "/CloudApi/Add";
        var content =
        {
            CreatorUserId: userId,
            AccountType: 3,
            AccountName: name,
            GcProjectId: projectId,
            GcJsonBody: keyJson
        };
        var messageParagraph = $("#add_google_account_form").find("#messages");
        messageParagraph.InnerHTML = "";
        var jsonMessage = JSON.stringify(content);

        $.ajax({
            type: "post",
            dataType: "html",
            contentType: "application/json",
            url: apiUrl,
            data: jsonMessage,
            success: function () {
                messageParagraph.html("Your new Google Cloud account has been added.");
                messageParagraph.attr("class", "green");
            },
            error: function (jqXhr, textStatus, errorThrown) {
                messageParagraph.html(xhr.status + ': ' + xhr.statusText);
                messageParagraph.attr("class", "red");
            }
        });
    },
    TestConnection: function (projectId, keyJson, userId, name) {
        var apiUrl = "/CloudApi/Test";

        var content =
        {
            CreatorUserId: userId,
            AccountType: 3,
            AccountName: name,
            GcProjectId: projectId,
            GcJsonBody: keyJson
        };

        var jsonMessage = JSON.stringify(content);
        var messageParagraph = $("#add_aws_account_form").find("#messages");
        messageParagraph.InnerHTML = "";

        $.ajax({
            type: "post",
            dataType: "html",
            contentType: "application/json",
            url: apiUrl,
            data: jsonMessage,
            success: function () {
                messageParagraph.html("Test Succeeded!");
                messageParagraph.attr("class", "green");
            },
            error: function (jqXhr, textStatus, errorThrown) {
                messageParagraph.html(xhr.status + ': ' + xhr.statusText);
                messageParagraph.attr("class", "red");
            }
        });
    },

    LoadAddGoogleCloudAccountForm: function() {
        $('#add-aws-modal').modal('show').find('.modal-body').load('/google/add');

        $('#add-aws-modal').on('hide.bs.modal',
            function() {
                location.reload(true);
            });
    }
}