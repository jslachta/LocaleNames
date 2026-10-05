# LocaleNames

[![NuGet](https://img.shields.io/nuget/v/LocaleNames.svg)](https://www.nuget.org/packages/LocaleNames/)
[![Downloads](https://img.shields.io/nuget/dt/LocaleNames.svg)](https://www.nuget.org/packages/LocaleNames/)
[![Coverage Status](https://coveralls.io/repos/github/jslachta/LocaleNames/badge.svg?branch=master)](https://coveralls.io/github/jslachta/LocaleNames?branch=master)
[![CodeFactor](https://codefactor.io/repository/github/jslachta/localenames/badge)](https://codefactor.io/repository/github/jslachta/localenames)

Names of languages, countries and currencies in almost any language, for .NET. Look a name up by its code, or a code by its name.

The data comes from [Unicode CLDR](https://github.com/unicode-org/cldr-json), is embedded in the package, and needs no network access or OS culture data. Targets `netstandard2.0`, `net8.0` and `net10.0`.

## Install

```
dotnet add package LocaleNames
```

## Usage

Get a translations object for the language you want the names in, then query it.

```csharp
var cs = LocaleTranslationsFactory.ForLanguageCode("cs-CZ");
var en = LocaleTranslationsFactory.ForCultureInfo(new CultureInfo("en-US"));
```

### Languages

```csharp
en.FindLanguageName("cs-CZ");   // Czech (Czechia)
en.FindLanguageCode("Czech");   // cs
en.AllLanguageCodes;
```

### Countries

```csharp
en.FindCountryName("DE");       // Germany
en.FindCountryCode("Germany");  // DE
en.AllCountryCodes;
```

### Currencies

```csharp
cs.FindCurrencyName("CZK");                      // česká koruna
cs.FindCurrencyName("CZK", PluralCategory.Few);  // české koruny
cs.FindCurrencyPluralNames("CZK");               // all plural forms
cs.FindCurrencyCode("česká koruna");             // CZK

en.FindCurrencySymbol("CZK");                       // CZK
en.FindCurrencySymbol("CZK", AltVariant.Narrow);    // Kč
en.GetAllCurrencyCodes();
```

## Contributing

Issues and pull requests are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md).

## Activity

![Repobeats](https://repobeats.axiom.co/api/embed/864145fa59a424553c94a73d2343776612860b15.svg "Repobeats analytics image")
