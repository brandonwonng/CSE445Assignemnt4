using System;
using System.Xml.Schema;
using System.Xml;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/**
 * This template file is created for ASU CSE445 Distributed SW Dev Assignment 4.
 * Please do not modify or delete any existing class/variable/method names. However, you can add more variables and functions.
 * Uploading this file directly will not pass the autograder's compilation check, resulting in a grade of 0.
 * **/
namespace ConsoleApp1
{
    public class Program
    {
        public static string xmlURL = "https://brandonwonng.github.io/CSE445Assignemnt4/Hotels.xml";
        public static string xmlErrorURL = "https://brandonwonng.github.io/CSE445Assignemnt4/HotelsErrors.xml";
        public static string xsdURL = "https://brandonwonng.github.io/CSE445Assignemnt4/Hotels.xsd";

        public static void Main(string[] args)
        {
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);

            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);

            result = Xml2Json(xmlURL);
            Console.WriteLine(result);
        }

        // Q2.1
        public static string Verification(string xmlUrl, string xsdUrl)
        {
            var messages = new System.Collections.Generic.List<string>();

            try
            {
                var schemas = new XmlSchemaSet();
                using (var xsdReader = XmlReader.Create(xsdUrl))
                {
                    schemas.Add(null, xsdReader);
                }

                var settings = new XmlReaderSettings
                {
                    ValidationType = ValidationType.Schema,
                    Schemas = schemas,
                    DtdProcessing = DtdProcessing.Prohibit
                };

                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;

                settings.ValidationEventHandler += (sender, e) =>
                {
                    messages.Add($"{e.Severity}: {e.Message}");
                };

                using (var reader = XmlReader.Create(xmlUrl, settings))
                {
                    while (reader.Read()) {}
                }

                return messages.Count == 0 ? "No Error" : string.Join(Environment.NewLine, messages);
            }
            catch (XmlSchemaException ex)
            {
                return $"XSD Error: {ex.Message}";
            }
            catch (XmlException ex)
            {
                return $"XML Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // Q2.2
        public static string Xml2Json(string xmlUrl)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(xmlUrl);

                var hotelsArray = new JArray();

                var hotelNodes = doc.SelectNodes("/Hotels/Hotel");
                if (hotelNodes != null)
                {
                    foreach (XmlNode hotel in hotelNodes)
                    {
                        var hotelObj = new JObject();

                        var nameNode = hotel.SelectSingleNode("Name");
                        if (nameNode != null)
                        {
                            hotelObj["Name"] = nameNode.InnerText.Trim();
                        }

                        var phoneNodes = hotel.SelectNodes("Phone");
                        var phones = new JArray();
                        if (phoneNodes != null)
                        {
                            foreach (XmlNode p in phoneNodes)
                            {
                                var val = (p.InnerText ?? "").Trim();
                                if (val.Length > 0) phones.Add(val);
                            }
                        }
                        hotelObj["Phone"] = phones;

                        var addressNode = hotel.SelectSingleNode("Address");
                        if (addressNode != null)
                        {
                            var addrObj = new JObject
                            {
                                ["Number"] = addressNode.SelectSingleNode("Number")?.InnerText?.Trim() ?? "",
                                ["Street"] = addressNode.SelectSingleNode("Street")?.InnerText?.Trim() ?? "",
                                ["City"] = addressNode.SelectSingleNode("City")?.InnerText?.Trim() ?? "",
                                ["State"] = addressNode.SelectSingleNode("State")?.InnerText?.Trim() ?? "",
                                ["Zip"] = addressNode.SelectSingleNode("Zip")?.InnerText?.Trim() ?? ""
                            };

                            var addressElem = addressNode as XmlElement;
                            if (addressElem != null && addressElem.HasAttribute("NearestAirport"))
                            {
                                var na = addressElem.GetAttribute("NearestAirport");
                                if (!string.IsNullOrWhiteSpace(na))
                                {
                                    addrObj["_NearestAirport"] = na.Trim();
                                }
                            }

                            hotelObj["Address"] = addrObj;
                        }

                        var hotelElem = hotel as XmlElement;
                        if (hotelElem != null && hotelElem.HasAttribute("Rating"))
                        {
                            var rating = hotelElem.GetAttribute("Rating");
                            if (!string.IsNullOrWhiteSpace(rating))
                            {
                                hotelObj["_Rating"] = rating.Trim();
                            }
                        }

                        hotelsArray.Add(hotelObj);
                    }
                }

                var root = new JObject
                {
                    ["Hotels"] = new JObject
                    {
                        ["Hotel"] = hotelsArray
                    }
                };

                var jsonText = root.ToString(Newtonsoft.Json.Formatting.None);
                try
                {
                    var _ = JsonConvert.DeserializeXmlNode(jsonText);
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException("Generated JSON could not be parsed by Newtonsoft as XML. Check key names and structure.", ex);
                }

                return jsonText;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
