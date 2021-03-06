﻿'use strict';
define(['app', 'Settings/Naming/NamingModel'], function () {

    NzbDrone.Settings.Naming.NamingView = Backbone.Marionette.ItemView.extend({
        template : 'Settings/Naming/NamingTemplate',
        className: 'form-horizontal',

        ui: {
            tooltip: '[class^="help-inline"] i'
        },

        initialize: function () {
            this.model = new NzbDrone.Settings.Naming.NamingModel();
            this.model.fetch();

            NzbDrone.vent.on(NzbDrone.Commands.SaveSettings, this.saveSettings, this);
        },

        onRender: function () {
            this.ui.tooltip.tooltip({ placement: 'right' });
        },

        saveSettings: function () {
            this.model.save(undefined, this.syncNotification("Naming Settings Saved", "Couldn't Save Naming Settings"));
        },


        syncNotification: function (success, error) {
            return {
                success: function () {
                    window.alert(success);
                },

                error: function () {
                    window.alert(error);
                }
            };
        }
    });
})
;
