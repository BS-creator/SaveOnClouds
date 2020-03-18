var Schedule = (function () {
    'use strict';

    let _getScheduleUrl = "/api/Schedule/GetScheduleById/";
    let _createOrEditUrl = "/api/Schedule/CreateOrUpdateSchedule";
    let _getAllSchedulesUrl = "/api/Schedule/GetAll";
    let _checkNameExistsUrl = "/api/Schedule/Check/";

    let _data = {};
    let _precision = 1;

    let _generateSchedulesTable = function (schedules) {
        $(".schedule-list").remove();
        let isFirst = true;
        let table = $("<table>").addClass("table schedule-list");
        let hide = ["data", "precision"];
        schedules = schedules || [];
        schedules.forEach(function (schedule) {
            let row = $("<tr>");
            let headerRow = $("<tr>");
            let entries = Object.entries(schedule);
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
                `<a href="#" data-id="${schedule.id}" 
                            data-url="/schedule/edit/${schedule.id}" 
                            class="openEditScheduleModal">Edit</a> |
                 <a href="api/schedule/Delete/${schedule.id}" 
                onclick="return confirm('Are you sure you want to delete this schedule?');">Delete</a>`
            ));
            table.append(row);
        });

        $(".schedule-list-wrapper").append(table);
    };

    let _fillEditForm = function (schedule) {
        let entries = Object.entries(schedule);
        for (const [key, val] of entries) {
            var capitalKey = key.charAt(0).toUpperCase() + key.substring(1);
            let el = $("#" + capitalKey);
            if (el.is("input[type=checkbox]"))
                el.attr("checked", val);
            else
                el.val(val);
        }
    };

    let _createSchedule = function () {
        $.fn.scheduler.locales['EN'] = {
            WEEK_DAYS: ['MON', 'TUE', 'WED', 'THU', 'FRI', 'SAT', 'SUN'],
            DRAG_TIP: 'Drag to select hours',
            RESET: 'Reset Selected'
        };

        $('.scheduleTable').scheduler({
            data: _data,
            accuracy: _precision,
            locale: 'EN',
            footer: true,
            onSelect: function (d) {
                _data = d;
            }
        });
    };

    let _updateData = function (precision) {
        let prevPrecision = _precision;
        let prevData = _data;

        _precision = precision;
        _data = {};

        let entries = Object.entries(prevData);
        for (const [row, selected] of entries) {
            let changed = [];
            for (let i = 0; i < selected.length; i++) {
                for (let j = 0; j < precision / prevPrecision; j++) {
                    let newValue = selected[i] * (precision / prevPrecision) + j;
                    changed.push(Math.trunc(newValue));
                }
            }
            _data[row] = changed.filter((v, i, a) => a.indexOf(v) === i);
        }
    };

    let _transformToScheduleDetails = function () {
        let data = { "DayOfWeeks": [] };

        let entries = Object.entries(_data);
        for (const [key, val] of entries) {
            for (var i = 0; i < val.length; i++) {
                let hourIndex = Math.trunc(val[i] / _precision);
                let quarters = [true, true, true, true];
                
                if (_precision === 2) {
                    let first = val.indexOf(hourIndex * _precision) > -1;
                    let second = val.indexOf(hourIndex * _precision + 1) > -1;
                    if (second) i++;

                    quarters = [first, first, second, second];
                }
                if (_precision === 4) {
                    let first = val.indexOf(hourIndex * _precision) > -1;
                    let second = val.indexOf(hourIndex * _precision + 1) > -1;
                    let third = val.indexOf(hourIndex * _precision + 2) > -1;
                    let fourth = val.indexOf(hourIndex * _precision + 3) > -1;

                    if (second) i++;
                    if (third) i++;
                    if (fourth) i++;

                    quarters = [first, second, third, fourth];
                }

                data.DayOfWeeks.push({
                    "DayIndex": key-1,
                    "Hours": [{
                        "HourIndex": hourIndex,
                        "Quarter1": quarters[0],
                        "Quarter2": quarters[1],
                        "Quarter3": quarters[2],
                        "Quarter4": quarters[3]
                    }]
                });
            }
        }

        return data;
    };

    let _transformToData = function (data) {
        _precision = 4;
        _data = {};
        let leastPrec = 1;

        let scheduleDetails = JSON.parse(data);
        scheduleDetails.DayOfWeeks.forEach(function (d) {
            _data[d.DayIndex + 1] = _data[d.DayIndex + 1] || [];
            d.Hours.forEach(function (h) {
                if (h.Quarter1) {
                    _data[d.DayIndex + 1].push(h.HourIndex * _precision);
                }
                if (h.Quarter2) {
                    _data[d.DayIndex + 1].push(h.HourIndex * _precision + 1);
                }
                if (h.Quarter3) {
                    _data[d.DayIndex + 1].push(h.HourIndex * _precision + 2);
                }
                if (h.Quarter4) {
                    _data[d.DayIndex + 1].push(h.HourIndex * _precision + 3);
                }
                if (!h.Quarter1 || !h.Quarter2 || !h.Quarter3 || !h.Quarter4) {
                    if (!h.Quarter1 && !h.Quarter2 && h.Quarter3 === h.Quarter4 ||
                        h.Quarter1 === h.Quarter2 && (!h.Quarter3 && !h.Quarter4)) {
                        leastPrec = Math.max(leastPrec, 2);
                    } else
                        leastPrec = 4;
                }
            });
        });

        if (leastPrec < _precision) {
            _updateData(leastPrec);
        }
    };

    let _createOrEditFormSubmit = function () {
        $('#editScheduleModal').on("click", ".submit-schedule", function (e) {
            e.preventDefault();
            let form = $(this).parents("form");

            if (!form.valid())
                return;

            let data = _transformToScheduleDetails();
            let ser = form.serialize();
            ser += "&Data=" + JSON.stringify(data);

            $.ajax({
                url: _createOrEditUrl,
                type: 'POST',
                data: ser
            }).done(function (res) {
                _getAllSchedules();
                $('#editScheduleModal').modal('hide');
            });
        });
    };

    let _handlePrecisionChange = function () {
        $('#editScheduleModal').on("change", ".precision", function () {
            _updateData(parseInt($(this).val()));

            $('.scheduleTable').remove();
            var table = $('<table>').addClass('scheduleTable');
            $('.scheduleTableWrapper').append(table);
            _createSchedule();
        });
    };

    let initEdit = function (id) {
        id = id || "";
        $.get(_getScheduleUrl + id, function (schedule) {
            _data = {};
            _precision = 1;

            if (schedule) {
                _transformToData(schedule.data);

                schedule.precision = _precision;
                _fillEditForm(schedule);
            }

            _createSchedule();

            _handlePrecisionChange();
            
            _createOrEditFormSubmit();

            $('#editScheduleModal').on('hidden.bs.modal', function () {
                $('#editScheduleModal').off("click", ".submit-schedule");
                $('#editScheduleModal').off("change", ".precision");
            });
        });
    };

    let _getAllSchedules = function () {
        $.get(_getAllSchedulesUrl, function (schedules) {
            _generateSchedulesTable(schedules);
        });
    };

    let _uniqueNameValidator = function () {
        var response;
        $.validator.addMethod(
            "uniqueName",
            function (value, element) {
                $.get(_checkNameExistsUrl + value, function (res) {
                    response = !res.exists;
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
        _getAllSchedules();
        _uniqueNameValidator();

        $('.container').on("click", '.openEditScheduleModal', function (e) {
            e.preventDefault();
            let scheduleId = $(this).data('id');

            $.get($(this).data('url'), function (data) {
                initEdit(scheduleId);

                $('#editScheduleModal .modal-content').html(data);

                $('#editScheduleModal').modal('show');
            });
        });
    };

    let check = function (el) {
        $.get(_checkNameExistsUrl + el.value, function (data) {
            $(el).removeClass('error').removeAttr("aria-invalid");
            $(el).siblings("label#" + el.id + "-error").remove();
            el.setCustomValidity('');
            if (data.exists) {
                console.log("check");
                $(el).addClass('error').attr("aria-invalid", true);
                let error = $("<label id='" + el.id + "-error' class='error' for='" + el.id + "'>This name already exists!</label>");
                $(el).after(error);
                el.setCustomValidity('This name already exists!');
            }
        });
    };

    

    return {
        Init: init,
        Check: check
    };
})();