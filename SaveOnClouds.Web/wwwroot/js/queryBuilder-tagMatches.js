!function ($) {
  var e = ["equal", "not_equal"]
    , c = "tag_matches";
  $.fn.queryBuilder.define("tag-matches", function (v) {
    function updateValue(e) {
      e._updating_input || (e._updating_value = !0,
        e.value = e.filter.valueGetter(e),
        e._updating_value = !1)
    }
    function r(e) {
      var t = e.$el.find("select[name$='_value_select_1']")
        , l = e.$el.find("input[name$='_value_input_1']");
      "is_empty" === t.val() || "is_not_empty" === t.val() ? (l.val(""),
        l.hide()) : l.show();
      var n = e.$el.find("select[name$='_value_select_2']")
        , a = e.$el.find("input[name$='_value_input_2']");
      "is_empty" === n.val() || "is_not_empty" === n.val() ? (a.val(""),
        a.hide()) : a.show()
    }
    this.operators.push({
      type: c,
      nb_inputs: 1,
      multiple: !1,
      apply_to: ["string"]
    }),
      0 === v.key_operators.length && (v.key_operators = e),
      0 === v.value_operators.length && (v.value_operators = e),
      this.on("getRuleOperatorSelect.filter", function (e, t) {
        !!t.filter.operators && -1 !== t.filter.operators.indexOf(c) && (e.value = "")
      }),
      this.on("getRuleInput.filter", function (e, t) {
        if (!!t.filter.operators && -1 !== t.filter.operators.indexOf(c)) {
          for (var l = {
            display: "inline-block",
            width: "50px",
            "font-weight": "bold",
            "margin-left": "5px"
          }, n = {
            "margin-left": "5px",
            "margin-top": "2px",
            "margin-bottom": "2px"
          }, a = $("<span/>", {
            name: t.id + "_value_label_1",
            text: "Key"
          }).css(l), i = $("<select/>", {
            name: t.id + "_value_select_1",
            class: "form-control tag-name"
          }), r = $("<input/>", {
            name: t.id + "_value_input_1",
            type: "text"
          }).css(n), o = 0; o < v.key_operators.length; o++)
            i.append($("<option/>", {
              value: v.key_operators[o],
              text: e.builder.lang.operators[v.key_operators[o]]
            }));
          var u = $("<span/>", {
            name: t.id + "_value_label_2",
            text: "Value"
          }).css(l)
            , p = $("<select/>", {
              name: t.id + "_value_select_2",
              class: "form-control tag-name"
            })
            , _ = $("<input/>", {
              name: t.id + "_value_input_2",
              type: "text"
            }).css(n);
          for (o = 0; o < v.value_operators.length; o++)
            p.append($("<option/>", {
              value: v.value_operators[o],
              text: e.builder.lang.operators[v.value_operators[o]]
            }));
          var s = $("<div/>").css({
            display: "inline-block"
          });
          s.append(a, i, r),
            s.append($("<br>")),
            s.append(u, p, _),
            e.value = s.prop("outerHTML")
        }
      }),
      this.on("setRules.filter", function (e) {
        e.value.rules.forEach(function (rule) {
          if (rule.operator === "tag_matches") {
            rule.rules = undefined
          }
        })
      }),
      this.on("ruleToJson.filter", function (e, rule) {
        if (rule.operator.type === "tag_matches") {
          e.value.rules = [e.value.value.key, e.value.value.value],
          e.value.condition = "OR"
        }
      }),
      this.on("afterCreateRuleInput.filter", function (e, t) {
        !!t.filter.operators && -1 !== t.filter.operators.indexOf(c) && (t.filter.valueGetter = function (e) {
          var t = e.$el.find("select[name$='_value_select_1']")
            , l = e.$el.find("input[name$='_value_input_1']")
            , n = t.val()
            , a = "is_empty" === n || "is_not_empty" === n ? null : l.val().trim()
            , i = e.$el.find("select[name$='_value_select_2']")
            , r = e.$el.find("input[name$='_value_input_2']")
            , o = i.val();
          return {
            key: {
              id: v.key_id,
              field: v.key_id,
              type: "string",
              input: "text",
              operator: n,
              value: a
            },
            value: {
              id: v.value_id,
              field: v.value_id,
              type: "string",
              input: "text",
              operator: o,
              value: "is_empty" === o || "is_not_empty" === o ? null : r.val().trim()
            }
          }
        }
          ,
          t.filter.valueSetter = function (e, t) {
            var l = e.$el.find("select[name$='_value_select_1']")
              , n = e.$el.find("input[name$='_value_input_1']");
            l.find("option[value='" + t.key.operator + "']").prop("selected", !0),
              n.val(t.key.value || "");
            var a = e.$el.find("select[name$='_value_select_2']")
              , i = e.$el.find("input[name$='_value_input_2']");
            a.find("option[value='" + t.value.operator + "']").prop("selected", !0),
              i.val(t.value.value || ""),
              r(e)
          }
          ,
          t.filter.validation = t.filter.validation || {},
          t.filter.validation.callback = function (e) {
            var t = e.key;
            e = e.value;
            return t.value || "is_empty" === t.operator || "is_not_empty" === t.operator ? !(!e.value && "is_empty" !== e.operator && "is_not_empty" !== e.operator) || ["Tag value text cannot be empty"] : ["Tag name text cannot be empty"]
          }
          ,
          t.$el.find(".rule-value-container select").each(function () {
            $(this).off(v.key_input_event + "." + c),
              $(this).on(v.key_input_event + "." + c, function () {
                r(t),
                  updateValue(t)
              })
          }),
          t.$el.find(".rule-value-container input").each(function () {
            $(this).off(v.value_input_event + "." + c),
              $(this).on(v.value_input_event + "." + c, function () {
                updateValue(t)
              })
          }))
      }),
      this.on("afterUpdateRuleValue.filter", function (e, t) {
        e.builder && e.builder.settings && e.builder.settings.is_disabled && ("select" !== t.filter.input && "text" !== t.filter.input || t.$el.find(".rule-filter-container select").prop("title", t.filter.label),
          "select" === t.filter.input ? (t.$el.find(".rule-operator-container select").prop("title", e.builder.lang.operators[t.operator.type]),
            t.$el.find(".rule-value-container select").prop("title", t.$el.find(".rule-value-container option:selected").text())) : "text" === t.filter.input && !!t.filter.operators && -1 === t.filter.operators.indexOf(c) ? (t.$el.find(".rule-operator-container select").prop("title", e.builder.lang.operators[t.operator.type]),
              t.$el.find(".rule-value-container input").prop("title", t.value)) : "text" === t.filter.input && !!t.filter.operators && -1 !== t.filter.operators.indexOf(c) && (t.$el.find(".rule-value-container select[name$=_select_1]").prop("title", e.builder.lang.operators[t.value.key.operator]),
                t.$el.find(".rule-value-container input[name$=_input_1]").prop("title", t.value.key.value || ""),
                t.$el.find(".rule-value-container select[name$=_select_2]").prop("title", e.builder.lang.operators[t.value.value.operator]),
                t.$el.find(".rule-value-container input[name$=_input_2]").prop("title", t.value.value.value || "")))
      })
  }, {
    key_input_event: "change",
    key_operators: [],
    value_input_event: "change",
    value_operators: []
  })
}($);