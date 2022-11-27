# Locale Names
[![NuGet](https://img.shields.io/nuget/v/LocaleNames.svg)](https://www.nuget.org/packages/LocaleNames/) 
[![NuGet](https://img.shields.io/nuget/dt/LocaleNames.svg)](https://www.nuget.org/packages/LocaleNames/)
[![Coverage Status](https://coveralls.io/repos/github/jslachta/LocaleNames/badge.svg?branch=master)](https://coveralls.io/github/jslachta/LocaleNames?branch=master)
[![CodeFactor](https://codefactor.io/repository/github/jslachta/localenames/badge)](https://codefactor.io/repository/github/jslachta/localenames)

.NET library providing localized language names and country names.

The translation data are generated from [CLDR locale data for internationalization](https://github.com/unicode-org/cldr-json "CLDR locale data for internationalization"). 

# Usage

### Find all language codes

```
var allLanguageCodes = LocaleTranslationsFactory.ForLanguageCode("en-US").AllLanguageCodes;
```

### Find language name

```
var translatedLanguageName = LocaleTranslationsFactory.ForCultureInfo(new CultureInfo("en-US")).FindLanguageName("cs-CZ");
```

### Find language code

```
var languageCode = LocaleTranslationsFactory.ForCultureInfo(new CultureInfo("en-US")).FindLanguageCode("Czech");
```

### Find all country codes

```
var allCountryCodes = LocaleTranslationsFactory.ForLanguageCode("en-US").AllCountryCodes;
```

### Find country name

```
var translatedCountryName = LocaleTranslationsFactory.ForCultureInfo(new CultureInfo("en-US")).FindCountryName("DE");
```

### Find country code

```
var countryCode = LocaleTranslationsFactory.ForCultureInfo(new CultureInfo("en-US")).FindCountryCode("Germany");
```
# Stats
![Alt](https://repobeats.axiom.co/api/embed/864145fa59a424553c94a73d2343776612860b15.svg "Repobeats analytics image")

# Contributing

Contributions are welcome. Feel free to file issues and pull requests on the repo.
