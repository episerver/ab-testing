define([
    "dojo/_base/declare",
    "dijit/_WidgetBase",
    "dijit/_TemplatedMixin",
    "dijit/_WidgetsInTemplateMixin",
    "dojo/text!./templates/WeightSelector.html",
    "dojo/dom-class",
    "dojo/on",
    "xstyle/css!marketing-testing/css/WeightSelector.css"
], function (declare, _WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin, template, domClass, on) {
    return declare("WeightSelector", [_WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin], {

        templateString: template,

        value: null,

        disabled: false,

        showLabel: true,

        _setValueAttr: function (value) {
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
            this._importanceSelected();
            this._setSelectorValueLabel();
        },

        _setSelectorValueLabel: function () {
            if (this.showLabel) {
                this.selectorValue.innerHTML = this.value;
            }
            else {
                dojo.destroy(this.selectorValue);
                domClass.remove(this.importanceHigh, "epi-weightSelector-high-padded");
            }
        },

        _importanceSelected: function (evt) {
            var weight = this.value;
            if (evt && !this.disabled) {
                weight = evt.currentTarget.id;
            }

            if (weight) {
                if (this.value != weight) {
                    this._setValueAttr(weight);
                }
                switch (weight) {
                    case "Low":
                        domClass.replace(this.importanceLow, "epi-weightSelector-kpiweight epi-weightSelector-kpiweight-low-selected");
                        domClass.replace(this.importanceMedium, "epi-weightSelector-kpiweight epi-weightSelector-kpiweight--default");
                        domClass.replace(this.importanceHigh, "epi-weightSelector-kpiweight epi-weightSelector-kpiweight--default epi-weightSelector-high-padded");
                        break;
                    case "Medium":
                        domClass.replace(this.importanceLow, "epi-weightSelector-kpiweight epi-weightSelector-kpiweight-low");
                        domClass.replace(this.importanceMedium, "epi-weightSelector-kpiweight epi-weightSelector-kpiweight-medium-selected");
                        domClass.replace(this.importanceHigh, "epi-weightSelector-kpiweight epi-weightSelector-kpiweight--default epi-weightSelector-high-padded");
                        break;
                    case "High":
                        domClass.replace(this.importanceLow, "epi-weightSelector-kpiweight epi-weightSelector-kpiweight-low");
                        domClass.replace(this.importanceMedium, "epi-weightSelector-kpiweight epi-weightSelector-kpiweight-medium");
                        domClass.replace(this.importanceHigh, "epi-weightSelector-kpiweight epi-weightSelector-kpiweight-high-selected epi-weightSelector-high-padded");
                        break;
                }
            }
        },
    });
});