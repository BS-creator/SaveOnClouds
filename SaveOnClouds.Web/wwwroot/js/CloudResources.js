var CloudResources = {
    StatusesTimer: {},
    Init: function () {
        $("#grid-cloud-resources").bootgrid({
            ajax: true,
            url: "/cloudresourcesapi",
            requestHandler: function (request) {
                var newRequest = {
                    currentPage: request.current,
                    pageSize: request.rowCount
                };
                if (!$.isEmptyObject(request.sort)) {
                    newRequest.sortingColumn = Object.keys(request.sort)[0];
                    newRequest.sortingDirection = request.sort[newRequest.sortingColumn];
                }
                return newRequest;
            },
            formatters: {
                "nameLink": function (column, row) {
                    return "<a href=\"#\" data-id=\"" + row.id + "\" class=\"cr-name-link\">" + row.name + "</a>";
                },
                "scheduleDropdown": function (column, row) {
                    return "<div data-id=\"" + row.id + "\" id=\"schedule-dropdown-" +
                        row.id +
                        "\" class=\"dropdown schedule-dropdown\"><button class=\"btn btn-primary btn-sm dropdown-toggle\" type=\"button\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">Select...</button><div class=\"dropdown-menu\" aria-labelledby=\"dropdownMenuButton\"></div></div>";
                },
                "commands": function (column, row) {
                    //return "<input type=\"checkbox\" class=\"chx-toggle\" data-size=\"small\" name=\"chx-toggle-" + row.id + "\" id=\"chx-toggle-" + row.id + "\"/>";
                    var checked = "";
                    if (row.status !== "Stopped" && row.status !== "Terminating") {
                        checked = "checked";
                    }
                    return "<input class=\"chx-toggle\" type=\"checkbox\" data-size=\"small\" data-on=\"<i class='fa fa-play'></i> Running\" data-off=\"<i class='fa fa-pause'></i> Stopped\" data-onstyle=\"success\" data-offstyle=\"danger\" data-row-id=\"" + row.id + "\" " + checked + ">";
                }
            }
        });

        $("#MetadataModal").on("hidden.bs.modal", CloudResources.OnMetadataModalClosed);

        $("#grid-cloud-resources").on("loaded.rs.jquery.bootgrid",
            function (e) {
                var schedules = JSON.parse($("#schedules-container").text());
                $.each(schedules,
                    function (index, obj) {
                        $(".dropdown-menu").append($("<a>").addClass("dropdown-item").attr("href", "#")
                            .data("id", obj.Id).data("name", obj.Name).data("description", obj.Description)
                            .html(obj.Name));
                    });

                $(".dropdown-item").on("click", CloudResources.OnScheduleSelected);

                $(".dropdown-toggle").dropdown();
                $(".chx-toggle").bootstrapToggle();
                $(".toggle-on").on("click",
                    function () {
                        CloudResources.ChangeStatus($(this), false);
                    });

                $(".toggle-off").on("click",
                    function () {
                        CloudResources.ChangeStatus($(this), true);
                    });
                $(".cr-name-link").on("click", CloudResources.OnNameClicked);

                $(".search.form-group").hide();

                CloudResources.SelectSchedules();

                CloudResources.StatusesTimer = setInterval(CloudResources.GetStatuses, 10 * 1000);
            });
    },

    SelectSchedules: function () {
        var rows = $("#grid-cloud-resources").bootgrid("getCurrentRows");
        $.each(rows,
            function (index, row) {
                var dd = $("#schedule-dropdown-" + row.id);

                var items = dd.find(".dropdown-item");

                $.each(items,
                    function (index, item) {
                        if ($(item).data("id") === row.scheduleId) {
                            var el = $(item);
                            el.addClass("active");
                            el.parent().parent().children().first().html(el.html());
                        }
                    });
            });
    },

    OnScheduleSelected: function () {
        var el = $(this);
        el.parent().children(".dropdown-item").removeClass("active");
        el.addClass("active");

        el.parent().removeClass("show");
        el.parent().parent().children().first().html(el.html());

        CloudResources.ChangeSchedule(el.data("id"), el.parent().parent().data("id"));
    },



    OnNameClicked: function () {
        var row = CloudResources.GetCurrentRow($(this).data("id"));
        CloudResources.FillTags(row.tags);
        

        $("#MetadataModal").modal("show", $(this));
    },

    FillTags: function (tags) {
        if (tags.length > 0) {
            var tagList = tags.split(",");
            var ulList = $("#tags-list");
            $.each(tagList, function (index, str) {
                ulList.append($("<li>").addClass("list-group-item").html(str));
            });
        }
    },


    GetCurrentRow: function (id) {
        var row = {};
        var rows = $("#grid-cloud-resources").bootgrid("getCurrentRows");
        $.each(rows,
            function (index, obj) {
                if (obj.id === id) {
                    row = obj;
                }
            });
        return row;
    },



    GetStatuses: function() {

    },

    ChangeStatus: function (element, starting) {
        var chk = $(element).parent().parent().find("input[type='checkbox']");

        var status = 2;
        if (starting) {
            status = 1;
        }

        var data = {
            Id: chk.data("row-id"),
            Status: status
        };

        $.ajax({
            type: "post",
            contentType: "application/json",
            url: "/CloudResourcesApi/SetStatus",
            data: JSON.stringify(data),
            success: function (response) {
                var toggleState = "off";
                if (starting) {
                    toggleState = "on";
                }
                $(chk).bootstrapToggle(toggleState, true);
                alert("message sent to change status");
            },
            error: function (jqXhr, textStatus, errorThrown) {
                alert("status not changed");
            }
        });
    },

    ChangeSchedule: function (resourceId, scheduleId) {
        var data = {
            ResourceId: resourceId,
            ScheduleId: scheduleId
        };
        $.ajax({
            type: "post",
            contentType: "application/json",
            url: "/CloudResourcesApi/SetSchedule",
            data: JSON.stringify(data),
            success: function (response) {
                alert("schedule changed");
            },
            error: function (jqXhr, textStatus, errorThrown) {
                alert("schedule not changed");
            }
        });
    }
};