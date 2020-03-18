var Environment = (function () {

  let _getAllSchedulesUrl = "/api/Schedule/GetAll";
  let _getEnvironmentUrl = "/api/Environment/GetEnvironmentById/";
  let _createOrEditUrl = "/api/Environment/CreateOrUpdateEnvironment";
  let _getAllEnvironmentsUrl = "/api/Environment/GetAll";
  let _checkNameExistsUrl = "/api/Environment/Check";
  let _queryUrl = "/api/Environment/QueryResources";

  let _schedules = [];

  let _generateEnvironmentsTable = function (environments) {
    $(".environment-list").remove();
    let isFirst = true;
    let table = $("<table>").addClass("table environment-list");
    let hide = ["queryJSON"];
    environments = environments || [];
    environments.forEach(function (environment) {
      let row = $("<tr>");
      let headerRow = $("<tr>");
      let entries = Object.entries(environment);
      for (const [key, val] of entries) {
        if (hide.indexOf(key) !== -1) {
          continue;
        }

        if (isFirst) {
          var capitalKey = key.charAt(0).toUpperCase() + key.substring(1);
          headerRow.append($("<th>").text(capitalKey));
        }

        row.append($("<td>").text(val));
      }
      if (isFirst) {
        headerRow.append($("<th>"));
        table.append(headerRow);
        isFirst = false;
      }
      row.append($("<td>").html(
        `<a href="#" data-id="${environment.id}"
          data-url="/api/environment/start/${environment.id}" 
          class="startStopEnvironment">Start</a> |
        <a href="#" data-id="${environment.id}"
          data-url="/api/environment/stop/${environment.id}" 
          class="startStopEnvironment">Stop</a> |
        <a href="#" data-id="${environment.id}"
          data-url="/environment/edit/${environment.id}" 
          class="openEditEnvironmentModal">Edit</a> |
        
        <a href="api/environment/Delete/${environment.id}" 
          onclick="return confirm('Are you sure you want to delete this environment?');">Delete</a>`
      ));
      table.append(row);
    });

    $(".environment-list-wrapper").append(table);
  };

  let _generateResourcesTable = function (resources) {
    $(".resource-list").remove();
    let isFirst = true;
    let table = $("<table>").addClass("table");
    let hide = ["cloudAccount"];
    resources = resources || [];
    resources.forEach(function (resource) {
      let row = $("<tr>");
      let headerRow = $("<tr>");
      let entries = Object.entries(resource);
      for (const [key, val] of entries) {
        if (hide.indexOf(key) !== -1) {
          continue;
        }

        if (isFirst) {
          var capitalKey = key.charAt(0).toUpperCase() + key.substring(1);
          headerRow.append($("<th>").text(capitalKey));
        }

        if (key === 'tags') {
          var tags = '';
          val.forEach(function (tag) {
            tags += `<${tag.key}:${tag.value}>; `;
          });
          row.append($("<td>").text(tags));
        } else {
          row.append($("<td>").text(val));
        }
      }
      if (isFirst) {
        table.append(headerRow);
        isFirst = false;
      }
      table.append(row);
    });

    let tableWrapper = table.wrap("div").addClass("table-responsive resource-list");

    $(".resource-list-wrapper").append(tableWrapper);
  };

  let _fillScheduleOptions = function () {
    var options = [];
    options.push($(`<option value="0">--- Select schedule ---</option>`));
    _schedules.forEach(function (schedule) {
      options.push($(`<option value="${schedule.id}">${schedule.name}</option>`));
    });
    $('#editEnvironmentModal #ScheduleId').append(options);
  };

  let _fillEditForm = function (environment) {
    let entries = Object.entries(environment);
    for (const [key, val] of entries) {
      var capitalKey = key.charAt(0).toUpperCase() + key.substring(1);
      let el = $("#" + capitalKey);
      if (el.is("input[type=checkbox]"))
        el.attr("checked", val);
      else
        el.val(val);
    }
  };

  let _getAllEnvironments = function () {
    $.get(_getAllEnvironmentsUrl, function (environments) {
      _generateEnvironmentsTable(environments);
    });
  };

  let _getAllSchedules = function () {
    $.get(_getAllSchedulesUrl, function (schedules) {
      _schedules = schedules;
    });
  };

  let _createOrEditFormSubmit = function () {
    $('#editEnvironmentModal').on("click", ".submit-environment", function (e) {
      e.preventDefault();
      let form = $(this).parents("form");

      if (!form.valid())
        return;

      var rules = $('#builder').queryBuilder('getRules');
      if (!rules.valid)
        return;

      //let data = _transformToEnvironmentDetails();
      let ser = form.serialize();
      ser += "&QueryJSON=" + JSON.stringify(rules);

      $.ajax({
        url: _createOrEditUrl,
        type: 'POST',
        data: ser
      }).done(function (res) {
        _getAllEnvironments();
        $('#editEnvironmentModal').modal('hide');
      });
    });
  };

  let _initEdit = function (id, htmlData) {
    id = id || "";
    $.get(_getEnvironmentUrl + id, function (environment) {

      $('#editEnvironmentModal .modal-content').html(htmlData);

      _fillScheduleOptions();
      if (environment) {
        _fillEditForm(environment);
      }

      _createOrEditFormSubmit();

      $('#editEnvironmentModal').on("click", ".test-query", function (e) {
        e.preventDefault();
        var rules = $('#builder').queryBuilder('getRules');

        $.post(_queryUrl, rules, function (res) {
          _generateResourcesTable(res);
          console.log(JSON.stringify(res));
        });

      });

      $('#editEnvironmentModal').on('hidden.bs.modal', function () {
        $('#editEnvironmentModal').off("click", ".submit-environment");
        $('#editEnvironmentModal').off("click", ".test-query");
        $('#builder').queryBuilder('destroy');
      });

      $('#builder').queryBuilder({
        filters: [
          {
            id: 'Id',
            label: 'Resource Id',
            type: 'integer'
          },
          {
            id: 'Name',
            label: 'Resource Name',
            type: 'string'
          },
          {
            id: 'CloudAccount.AccountType',
            label: 'Cloud Type',
            type: 'integer',
            input: 'select',
            values: {
              1: 'Amazon Web Services',
              2: 'Microsoft Azure'
            }
          },
          {
            id: 'Tags.Key',
            label: 'At Least One Tag Key',
            type: 'string'
          },
          {
            id: 'Tags.Value',
            label: 'At Least One Tag Value',
            type: 'string'
          },
          {
            id: 'Tags.Key_Value',
            label: 'At Least One Tag Key/Value',
            type: 'string',
            operators: ["tag_matches"]
          },
          {
            id: 'CloudAccounts.AccountName',
            label: 'Account Name',
            type: 'string'
          },
          {
            id: 'State',
            label: 'State',
            type: 'string'
          },
          {
            id: 'Location',
            label: 'Location',
            type: 'string'
          }
        ],
        plugins: {
          "tag-matches": {
            key_operators: ["equal", "not_equal", "begins_with", "not_begins_with", "contains", "not_contains", "ends_with", "not_ends_with", "is_empty", "is_not_empty"],
            value_operators: ["equal", "not_equal", "begins_with", "not_begins_with", "contains", "not_contains", "ends_with", "not_ends_with", "is_empty", "is_not_empty"],
            key_id: 'Tags.Key',
            value_id: 'Tags.Value',
            value_input_event: "keyup"
          }
        }
      });

      var rules = JSON.parse(environment.queryJSON);
      if (rules) {
        $('#builder').queryBuilder('setRules', JSON.parse(environment.queryJSON));
      } else {
        //$('#builder').queryBuilder('setRules', JSON.parse(environment.queryJSON));
      }
    });
  };

  let _uniqueNameValidator = function () {
    $.validator.addMethod(
      "uniqueName",
      function (value, element) {
        var id = $(element).parents('form').find('#Id').val();
        var response = false;
        $.ajax({
          url: `${_checkNameExistsUrl}/${value}/${id}`,
          success: function (res) {
            response = !res.exists;
          },
          async: false
        });
        return response;
      },
      "This name already exists!"
    );

    jQuery.validator.addClassRules({
      uniqueName: { uniqueName: true }
    });
  };

  let init = function () {
    _getAllEnvironments();
    _getAllSchedules();
    _uniqueNameValidator();

    $('.container').on("click", '.openEditEnvironmentModal', function (e) {
      e.preventDefault();
      let environmentId = $(this).data('id');

      $.get($(this).data('url'), function (htmlData) {
        _initEdit(environmentId, htmlData);

        $('#editEnvironmentModal').modal('show');
      });
    });
    $('.container').on("click", '.startStopEnvironment', function (e) {
      e.preventDefault();
      $.post($(this).data('url'), function (result) {
        alert(result.message);
      });
    });
  };

  return {
    Init: init
  };
})();