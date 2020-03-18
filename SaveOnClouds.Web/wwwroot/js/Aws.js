var Aws = {
    Bind: function () {
        $("#btnTestConnection").click = function() {
            var arn = $("#RoleArn").val();
            var externalId = $("#UserId").val();
            var name = $("#Name").val();
            TestAwsConnection(arn, externalId, name);
        };

        $("#btnCreate").bind("click",
            function() {
                var arn = $("#RoleArn").val();
                var name = $("#Name").val();
                var externalId = $("#UserId").val();
                Aws.CreateAccount(externalId, name, arn);
            });
    },
    TestAwsConnection: function(arn, externalId, name) {
        var apiUrl = "/CloudApi/Test";

        var content =
        {
            CreatorUserId: externalId,
            AccountType: 1,
            AccountName: name,
            AwsRoleArn: arn,
            AwsExternalId: externalId
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
    CreateAccount: function (externalId, accountName, roleArn) {
        var apiUrl = "/CloudApi/Add";
        var content =
        {
            CreatorUserId: externalId,
            AccountType: 1,
            AccountName: accountName,
            AwsRoleArn: roleArn,
            AwsExternalId: externalId
        };
        var messageParagraph = $("#add_aws_account_form").find("#messages");
        messageParagraph.InnerHTML = "";
        var jsonMessage = JSON.stringify(content);
        
        $.ajax({
            type: "post",
            dataType: "html",
            contentType: "application/json",
            url: apiUrl,
            data: jsonMessage,
            success: function () {
                messageParagraph.html("Your new AWS account has been added.");
                messageParagraph.attr("class", "green");
            },
            error: function (jqXhr, textStatus, errorThrown) {
                messageParagraph.html("This account has already been registered. If you do not see this account in the list of your accounts, it might have been registered under a different account.");
                messageParagraph.attr("class", "red");
            }
        });
    },
 
    DeleteAccount: function(accountId) {
        var apiUrl = "/CloudApi/Delete/" + accountId;
        $.ajax({
            type: "delete",
            url: apiUrl,
            success: function (data, textStatus, jQxhr) {
                alert('Account has been deleted');
                location.reload(true);
            },
            error: function (jqXhr, textStatus, errorThrown) {
                alert(jqXhr.responseText);
            }
        });
    },
    LoadAddAwsAccountForm: function() {
        $('#add-aws-modal').modal('show').find('.modal-body').load('/aws/add');

        $('#add-aws-modal').on('hide.bs.modal', function () {
            location.reload(true);
        });
    }
}