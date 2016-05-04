define([
     'dojo/_base/declare',
    'dijit/_WidgetBase',
    'dijit/_TemplatedMixin',
    'dojo/text!marketing-testing/views/MultivariateTestDetailsView.html',
    'dijit/_WidgetsInTemplateMixin'
    
], function (
    declare,
    _WidgetBase,
    _TemplatedMixin,
    template,
    _WidgetsInTemplateMixin
    

) {
    
    return declare([_WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin], {
        templateString: template,

        
    });
});