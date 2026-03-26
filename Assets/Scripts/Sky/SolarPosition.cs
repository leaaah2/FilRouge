using System;
using UnityEngine;

public static class SolarPosition
{
    public static Vector3 SunDirectionENU(double latitudeDeg, double longitudeDeg, DateTimeOffset localTime)
    {
        int dayOfYear = localTime.DayOfYear;
        double hour = localTime.Hour + localTime.Minute / 60.0 + localTime.Second / 3600.0;

        double gamma = 2.0 * Math.PI / 365.0 * (dayOfYear - 1 + (hour - 12.0) / 24.0);

        double eqTime =
            229.18 * (0.000075
                      + 0.001868 * Math.Cos(gamma)
                      - 0.032077 * Math.Sin(gamma)
                      - 0.014615 * Math.Cos(2 * gamma)
                      - 0.040849 * Math.Sin(2 * gamma));

        double decl =
            0.006918
            - 0.399912 * Math.Cos(gamma)
            + 0.070257 * Math.Sin(gamma)
            - 0.006758 * Math.Cos(2 * gamma)
            + 0.000907 * Math.Sin(2 * gamma)
            - 0.002697 * Math.Cos(3 * gamma)
            + 0.00148 * Math.Sin(3 * gamma);

        double tzMinutes = localTime.Offset.TotalMinutes;
        double timeOffset = eqTime + 4.0 * longitudeDeg - tzMinutes;

        double tst = hour * 60.0 + timeOffset;
        tst = (tst % 1440.0 + 1440.0) % 1440.0;

        double haDeg = tst / 4.0 - 180.0;
        double ha = haDeg * Math.PI / 180.0;

        double lat = latitudeDeg * Math.PI / 180.0;

        double cosZenith = Math.Sin(lat) * Math.Sin(decl) + Math.Cos(lat) * Math.Cos(decl) * Math.Cos(ha);
        cosZenith = Math.Clamp(cosZenith, -1.0, 1.0);

        double zenith = Math.Acos(cosZenith);
        double altitude = Math.PI / 2.0 - zenith;

        double sinAz = -Math.Sin(ha) * Math.Cos(decl);
        double cosAz = Math.Cos(lat) * Math.Sin(decl) - Math.Sin(lat) * Math.Cos(decl) * Math.Cos(ha);
        double azimuth = Math.Atan2(sinAz, cosAz);
        if (azimuth < 0) azimuth += 2.0 * Math.PI;

        float xEast = (float)(Math.Cos(altitude) * Math.Sin(azimuth));
        float yUp = (float)Math.Sin(altitude);
        float zNorth = (float)(Math.Cos(altitude) * Math.Cos(azimuth));

        return new Vector3(xEast, yUp, zNorth).normalized;
    }

    public static void ApplyToDirectionalLight(Light lightSource, Vector3 sunDirectionENU)
    {
        if (lightSource == null) return;
        lightSource.transform.rotation = Quaternion.LookRotation(-sunDirectionENU, Vector3.up);
    }
}