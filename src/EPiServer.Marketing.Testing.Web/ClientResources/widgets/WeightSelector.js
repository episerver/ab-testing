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
            this._setSelectorValueLabel();
            this._adjustImportanceSelectors();
        },

        _setSelectorValueLabel: function () {
            this.selectorValue.innerHTML = this.value;
        },

        _adjustImportanceSelectors: function () {
            if (this.value) {
                switch (this.value) {
                    case "Low":
                        this._importanceClicked();
                        this.importanceLow.checked = true;
                        break;
                    case "Medium":
                        this._importanceClicked();
                        this.importanceMedium.checked = true;
                        break;
                    case "High":
                        this._importanceClicked();
                        this.importanceHigh.checked = true;
                        break;
                    default:
                        console.debug("KpiWeightWidget init: " + this.value + " is not a valid weight value.");
                }
            }
        },

        _importanceClicked: function (evt) {
            var weight = this.value;
            var me = this;
            if (evt) {
                weight = evt.srcElement.value;
            }
            switch (weight) {
                case "Low":
                    if (me.value && me.value != "Low") {
                        this._setValueAttr("Low");
                    }
                    this.importanceMedium.checked = false;
                    domClass.replace(this.importanceMedium, "epi-weightSelector-kpiweight epi-weightSelector-kpiweight--default");
                    this.importanceHigh.checked = false;
                    domClass.replace(this.importanceHigh, "epi-weightSelector-kpiweight epi-weightSelector-kpiweight--default");
                    break;
                case "Medium":
                    if (this.value && this.value != "Medium") {
                        this._setValueAttr("Medium");
                    }
                    this.importanceLow.checked = false;
                    domClass.replace(this.importanceLow, "epi-weightSelector-kpiweight epi-weightSelector-kpiweight-low");
                    this.importanceHigh.checked = false;
                    domClass.replace(this.importanceHigh, "epi-weightSelector-kpiweight epi-weightSelector-kpiweight--default");
                    break;
                case "High":
                    if (this.value && this.value != "High") {
                        this._setValueAttr("High");
                    }
                    this.importanceLow.checked = false;
                    domClass.replace(this.importanceLow, "epi-weightSelector-kpiweight epi-weightSelector-kpiweight-low");
                    this.importanceMedium.checked = false;
                    domClass.replace(this.importanceMedium, "epi-weightSelector-kpiweight epi-weightSelector-kpiweight-medium");
                    break;
            }
        }
    });
});