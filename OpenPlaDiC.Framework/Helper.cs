using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OpenPlaDiC.Framework
{
    public static class Helper
    {

        public const double RADIO = 6371;
        public static double DistanceBetweenPlaces(double lat1, double lng1, double lat2, double lng2)
        {
            double distance = 0;
            double Lat = (lat2 - lat1) * (Math.PI / 180);
            double Lon = (lng2 - lng1) * (Math.PI / 180);
            double a = Math.Sin(Lat / 2) * Math.Sin(Lat / 2) + Math.Cos(lat1 * (Math.PI / 180)) * Math.Cos(lat2 * (Math.PI / 180)) * Math.Sin(Lon / 2) * Math.Sin(Lon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            distance = RADIO * c;
            return distance;
        }
        public static double DistanceBetweenPlacesGMaps(double lat1, double lng1, double lat2, double lng2)
        {
            double resp = 0;

            try
            {
                string origin = lat1.ToString() + "%2C" + lng1.ToString();
                string destination = lat2.ToString() + "%2C" + lng2.ToString();
                string url = "https://maps.googleapis.com/maps/api/distancematrix/xml?origins=" + origin + "&destinations=" + destination + "&key=AIzaSyALYTCoyaeDv77SvckXuNfDfP63QnrPlj0";
                WebRequest request = WebRequest.Create(url);
                using (WebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        DataSet dsResult = new DataSet();
                        dsResult.ReadXml(reader);
                        //duration.Text = dsResult.Tables["duration"].Rows[0]["text"].ToString();
                        //distance.Text = dsResult.Tables["distance"].Rows[0]["text"].ToString();
                        resp = Convert.ToDouble(dsResult.Tables["distance"].Rows[0]["value"]) / 1000;
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return resp;
        }
        public static string NormText(string text, bool noSpaces = true, bool upperCase = false)
        {
            var inputString = upperCase ? text.ToUpper() : text;
            var normalizedString = inputString.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            for (int i = 0; i < normalizedString.Length; i++)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(normalizedString[i]);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(normalizedString[i]);
                }
            }
            if (noSpaces)
                return (sb.ToString().Normalize(NormalizationForm.FormC).Replace("-", "_").Replace(" ", ""));
            else
                return (sb.ToString().Normalize(NormalizationForm.FormC).Replace("-", "_"));
        }
        public static string EncodePassword(string originalPassword)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] inputBytes = (new UnicodeEncoding()).GetBytes(originalPassword);
                byte[] hash = sha1.ComputeHash(inputBytes);

                return Convert.ToBase64String(hash);
            }
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public static string CreateRandomPassword(int passwordLength, bool justNumbers = false)
        {
            string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789#";

            if (justNumbers)
            {
                allowedChars = "0123456789";
            }

            char[] chars = new char[passwordLength];
            Random rd = new Random();

            for (int i = 0; i < passwordLength; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }
        public static string ToInitials(string word)
        {
            var sRes = "";
            foreach (var s in word.Split(' '))
            {
                sRes += s[0].ToString().ToUpper();
            }
            return sRes;
        }



    }
}
