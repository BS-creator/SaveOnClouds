var Accounts = {
    LoadCloudAccountList: function () {
        var grid = $("#grid-cloud-accounts").bootgrid({
            ajaxSettings: {
                method: "GET",
                cache: false
            },
            ajax: true,
            url: "/CloudApi/ListAccounts",
            formatters: {
                "commands": function (column, row) {
                    return "<button type=\"button\" class=\"btn btn-xs btn-default command-delete\" data-row-id=\"" + row.Id + "\"><img src='/icons/close.png'></img></button>";
                }
            }
        }).on("loaded.rs.jquery.bootgrid", function () {
            /* Executes after data is loaded and rendered */
            grid.find(".command-delete").on("click", function (e) {
                if (confirm(
                    "Are you sure you want to remove this cloud account? You must delete all the environments attached to a cloud account before deleting it") !== true)
                    return;

                Aws.DeleteAccount($(this).data("row-id"));
            });
        });



    }
}