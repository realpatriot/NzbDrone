"use strict";
define(['app'], function () {

    $.cookie.json = true;

    NzbDrone.Config.SeriesView = function (value) {
        if (value !== undefined) {
            NzbDrone.Config.SetValue('seriesView', value);
        }

        else{
            return NzbDrone.Config.GetValue('seriesView', 1);
        }
    };

    NzbDrone.Config.GetValue = function (key, defaultValue) {
        var cookie = NzbDrone.Config.GetCookie();

        if (!cookie) {
            return defaultValue;
        }

        var value = cookie[key];

        if (value === undefined) {
            return defaultValue;
        }

        else {
            return value;
        }
    };

    NzbDrone.Config.SetValue = function (key, value) {
        var cookie = NzbDrone.Config.GetCookie();

        if (!cookie) {
            cookie = {};
        }

        cookie[key] = value;
        NzbDrone.Config.SetCookie(cookie);
    };

    NzbDrone.Config.GetCookie = function () {
        return $.cookie('NzbDroneConfig');
    };

    NzbDrone.Config.SetCookie = function (cookie) {
        $.cookie('NzbDroneConfig', cookie, { expires: 365, path: '/' });
    };
});
