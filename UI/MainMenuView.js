﻿"use strict";
define(['app'], function () {
    NzbDrone.MainMenuView = Backbone.Marionette.ItemView.extend({
        events: {
            'click a': 'onClick'
        },

        onClick: function (event) {

            event.preventDefault();

            var target = $(event.target);

            //look down for <a/>
            var href = event.target.getAttribute('href');

            //if couldn't find it look up 
            if (!href && target.parent('a') && target.parent('a')[0]) {

                var linkElement = target.parent('a')[0];

                href = linkElement.getAttribute('href');
                this.setActive(linkElement);
            } else {
                this.setActive(event.target);
            }

            if (href && href.startsWith('http')) {
                window.location.href = href;
            } else {
                NzbDrone.Router.navigate(href, { trigger: true });
            }

        },

        setActive: function (element) {
            //Todo: Set active on first load
            this.$('a').removeClass('active');
            $(element).addClass('active');
        },

        initialize: function () {
            console.log('menu');
            this.setElement($('#main-menu-region'));
        }
    });


    return new NzbDrone.MainMenuView();
});
