define([
    "dojo/_base/declare",
    "dijit/_WidgetBase",
    "dijit/_TemplatedMixin",
    "dijit/_WidgetsInTemplateMixin",
    "dojo/text!./templates/WeightSelector.html",
    "dojo/dom-class",
    "dojo/on",
    'xstyle/css!marketing-testing/css/WeightSelector.css',
], function (declare, _WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin, template, domClass, on) {
    return declare("WeightSelector", [_WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin], {

        templateString: template,

        label: "",

        value: null,

        _setValueAttr(value) {
            this.value = value;
            this._init();
            on.emit(this, "change", {
                bubbles: true,
                cancelable: true
            })
        },

        postCreate: function () {
            this._init();
        },

        _init: function () {
            switch (this.value) {
                case "low":
                    this._lowImportanceClicked();
                    this.importanceLow.checked = true;
                    break;
                case "medium":
                    this._medImportanceClicked();
                    this.importanceMedium.checked = true;
                    break;
                case "high":
                    this._highImportanceClicked();
                    this.importanceHigh.checked = true;
                    break;
                default:
                    console.debug("KpiWeightWidget init: " + this.value + "Is not a valid weight value.");
            }
        },

        _lowImportanceClicked: function () {
            if (this.value && this.value != "low") {
                this._setValueAttr("low");
            }
            this.importanceMedium.checked = false;
            domClass.replace(this.importanceMedium, "epi-weightSelector-kpiweight epi-weightSelector-kpiweight--default");
            this.importanceHigh.checked = false;
            domClass.replace(this.importanceHigh, "epi-weightSelector-kpiweight epi-weightSelector-kpiweight--default");
        },

        _medImportanceClicked: function () {
            if (this.value && this.value != "medium") {
                this._setValueAttr("medium");
            }
            this.importanceLow.checked = false;
            domClass.replace(this.importanceLow, "epi-weightSelector-kpiweight epi-weightSelector-kpiweight-low");
            this.importanceHigh.checked = false;
            domClass.replace(this.importanceHigh, "epi-weightSelector-kpiweight epi-weightSelector-kpiweight--default");
        },

        _highImportanceClicked: function () {
            if (this.value && this.value != "high") {
                this._setValueAttr("high");
            }
            this.importanceLow.checked = false;
            domClass.replace(this.importanceLow, "epi-weightSelector-kpiweight epi-weightSelector-kpiweight-low");
            this.importanceMedium.checked = false;
            domClass.replace(this.importanceMedium, "epi-weightSelector-kpiweight epi-weightSelector-kpiweight-medium");
        }
    });
});