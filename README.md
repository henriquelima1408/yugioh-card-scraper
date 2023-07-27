# Yu-Gi-Oh! Card Scraper

![Yu-Gi-Oh! Logo](https://raw.githubusercontent.com/henriquelima1408/yugioh-card-scraper/master/yugioh-logo.png)

Welcome to the Yu-Gi-Oh! Card Database GitHub repository! This project aims to collect and provide a comprehensive dataset of information regarding all Yu-Gi-Oh! trading cards ever released. The repository allows users to access and download card data in various formats for research, analysis or any other purposes.

## Table of Contents

- [Introduction](#introduction)
- [Features](#features)
- [How it works?](#how-it-works)
- [Dataset](#dataset)
- [License](#license)

## Introduction

Yu-Gi-Oh! is a popular trading card game that involves dueling with a variety of unique and powerful cards. Over the years, thousands of cards have been released, making it challenging for players, collectors, and researchers to access all card-related information in one place. This repository aims to solve this problem by providing a well-structured dataset of Yu-Gi-Oh! cards.

## Features

- Comprehensive card database with details such as card name, type, attribute, level/rank, ATK/DEF points, effect text, and more.
- Ability to download the entire dataset or specific card data.
- Easy-to-use.
- Open to community contributions and improvements.

## How it works?

The application gathers card information from https://www.db.yugioh-card.com/ and retrieves all image resources from https://ygoprodeck.com/. In order to work properly, the application requires updates to the "MetadataCookie" and "DataCookies" within the "Args.json" file.


## Dataset

The dataset is available in the `data` directory. You can find the following directories:

- `CardMetada` - A collection of JSON files containing card ids.
- `CardData` - A collection of Card Images and Data (card name, type, attribute, level/rank, ATK/DEF points, etc ) in JSON format.

## License

The Yu-Gi-Oh! Card Database is released under the [MIT License](LICENSE). You are free to use, modify, and distribute the dataset for any legal purposes. However, please note that the dataset's content is subject to the terms and conditions set by Konami and the creators of Yu-Gi-Oh!.

---

Thank you for your interest in the Yu-Gi-Oh! Card Database project. We hope this dataset proves useful to Yu-Gi-Oh! fans, researchers, and enthusiasts. If you have any questions or suggestions, feel free to open an issue or contact me through the repository. Happy dueling!
