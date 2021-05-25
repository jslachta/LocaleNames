# Locale Names
[![NuGet](https://img.shields.io/nuget/v/LocaleNames.svg)](https://www.nuget.org/packages/LocaleNames//) 
[![NuGet](https://img.shields.io/nuget/dt/LocaleNames.svg)](https://www.nuget.org/packages/LocaleNames/)
[![Coverage Status](https://coveralls.io/repos/github/jslachta/LocaleNames/badge.svg?branch=master)](https://coveralls.io/github/jslachta/LocaleNames?branch=master)
[![CodeFactor](https://www.codefactor.io/repository/github/jslachta/localenames/badge)](https://www.codefactor.io/repository/github/jslachta/localenames)

This project aims to provide localized language names and country names.

The translation data are generated from [CLDR locale data for internationalization](https://github.com/unicode-cldr/cldr-localenames-full "CLDR locale data for internationalization"). 

# Usage

### Find language name

```
var translatedLanguageName = LocaleTranslations.ForCultureInfo(new CultureInfo("en-US")).FindLanguageName("cs-CZ");
```

### Find language code

```
var languageCode = LocaleTranslations.ForCultureInfo(new CultureInfo("en-US")).FindLanguageCode("Czech");
```

### Find country name

```
var translatedCountryName = LocaleTranslations.ForCultureInfo(new CultureInfo("en-US")).FindCountryName("DE");
```

### Find country code

```
var countryCode = LocaleTranslations.ForCultureInfo(new CultureInfo("en-US")).FindCountryCode("Germany");
```

# Contributing

Contributions are welcome.  Feel free to file issues and pull requests on the repo.
