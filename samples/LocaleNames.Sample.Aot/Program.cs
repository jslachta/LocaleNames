using System;
using LocaleNames;
using LocaleNames.Enumerations;
using System.Globalization;

var failures = 0;

void Check(string what, string expected, string actual)
{
    if (expected != actual)
    {
        failures++;
        Console.Error.WriteLine($"FAIL {what}: expected '{expected}', got '{actual}'");
    }
}

var cs = LocaleTranslationsFactory.ForCultureInfo(new CultureInfo("cs-CZ"));
var en = LocaleTranslationsFactory.ForCultureInfo(new CultureInfo("en-US"));
var enGb = LocaleTranslationsFactory.ForLanguageCode("en-GB");
var srLatn = LocaleTranslationsFactory.ForLanguageCode("sr-Latn-RS");

Check("cs country", "Německo", cs.FindCountryName("DE"));
Check("cs country code", "DE", cs.FindCountryCode("Německo"));
Check("cs language", "čeština", cs.FindLanguageName("cs"));
Check("cs currency", "česká koruna", cs.FindCurrencyName("CZK"));
Check("cs currency plural", "české koruny", cs.FindCurrencyName("CZK", PluralCategory.Few));
Check("cs currency code", "CZK", cs.FindCurrencyCode("česká koruna"));
Check("en country", "Germany", en.FindCountryName("DE"));
Check("en language", "Czech", en.FindLanguageName("cs-CZ"));
Check("en-GB (difference to en)", "Germany", enGb.FindCountryName("DE"));
Check("sr-Latn-RS", "Nemačka", srLatn.FindCountryName("DE"));
Check("all country codes", "True", (cs.GetAllCountryCodes().Count > 200).ToString());

Console.WriteLine(failures == 0 ? "AOT OK" : $"AOT FAILED ({failures})");

return failures == 0 ? 0 : 1;
