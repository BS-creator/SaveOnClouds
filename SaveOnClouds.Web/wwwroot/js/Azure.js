var Azure = {
    Bind: function () {
        $("#btnTestConnection").click = function () {
            var azureSubscriptionId = $("#AzureSubscriptionId").val();
            var azureTenantId = $("#AzureTenantId").val();
            var azureClientId = $("#AzureClientId").val();
            var azureClientSecret = $("#AzureClientSecret").val();
            var userId = $("#UserId").val();
            var name = $("#Name").val();
            Azure.TestConnection(name, userId, azureSubscriptionId, azureTenantId, azureClientId, azureClientSecret);
        };

        $("#btnCreate").bind("click",
            function () {
                var projectId = $("#ProjectId").val();
                var keyJson = $("#KeyJson").val();
                var userId = $("#UserId").val();
                var name = $("#Name").val();
                Azure.CreateAccount(projectId, keyJson, userId, name);
            });
    },

    CreateAccount: function (name, userId, azureSubscriptionId, azureTenantId, azureClientId, azureClientSecret) {
        var apiUrl = "/CloudApi/Add";
        var content =
        {
            CreatorUserId: userId,
            AccountType: 2,
            AccountName: name,
            AzureSubscriptionId: azureSubscriptionId,
            AzureTenantId: azureTenantId,
            AzureClientId: azureClientId,
            AzureClientSecret: azureClientSecret
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
                messageParagraph.html("Your new Azure account has been added.");
                messageParagraph.attr("class", "green");
            },
            error: function (jqXhr, textStatus, errorThrown) {
                messageParagraph.html(xhr.status + ': ' + xhr.statusText);
                messageParagraph.attr("class", "red");
            }
        });
    },
    TestConnection: function (name, userId, azureSubscriptionId, azureTenantId, azureClientId, azureClientSecret) {
        var apiUrl = "/CloudApi/Test";

        var content =
        {
            CreatorUserId: userId,
            AccountType: 2,
            AccountName: name,
            AzureSubscriptionId: azureSubscriptionId,
            AzureTenantId: azureTenantId,
            AzureClientId: azureClientId,
            AzureClientSecret: azureClientSecret
        };

        var jsonMessage = JSON.stringify(content);
        var messageParagraph = $("#add_azure_account_form").find("#messages");
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

    LoadAddAzureAccountForm: function() {
        $('#add-aws-modal').modal('show').find('.modal-body').load('/azure/add');

        $('#add-aws-modal').on('hide.bs.modal',
            function() {
                location.reload(true);
            });
    }
}