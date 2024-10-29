<div align="center">
  <h1><b>Bundestags-Mine</b></h1>
  <img src="https://github.com/TheItCrOw/Bundestags-Mine/assets/49918134/5c9b92df-4b05-4129-81ea-685430c10c19" width="250"/>
  <h3>Natural Language Processing for Extracting Key Information from Government Documents</h3>
  <hr/>
</div>

<i>As governments worldwide continue to release vast amounts of textual information, the need for efficient and insightful tools to extract, interpret and present this data has become increasingly critical. Towards solving this issue, we present the Bundestags-Mine: an environment that periodically retrieves pertinent data from the German parliament, parses and analyzes it using pipelines for natural language processing, and then displays the results in a web application that is publicly accessible. Bundestags-Mine helps to extract key information from parliamentary documents in a visually appealing matter for many use cases. For instance, the tool can be leveraged by journalists for news detection, lawyers for compliance checking, linguists for discourse analysis, and the broad public to inform themselves about the positions of political party members on a topic.</i>
<hr/>
<div align="center">
  <a href="https://bundestag-mine.de/"><img src="https://img.shields.io/static/v1?label=Live%3A&message=Website&color=2ea44f&style=for-the-badge" alt="Live: - Website"></a>
  <a href="https://bundestag-mine.de/"><img src="https://img.shields.io/static/v1?label=Languages%3A&message=German&color=informational&style=for-the-badge" alt="Languages: - German"></a><a href="https://www.paypal.com/donate/?hosted_button_id=3HC4L477XZRXU">
  <a href="https://ebooks.iospress.nl/doi/10.3233/FAIA230996"> <img src="https://img.shields.io/static/v1?label=Paper%3A&message=IOS+Press&color=important&style=for-the-badge&logo=researchgate" alt="Paper: - IOS Press"></a>
  <a href="https://bundestag-mine.de/researchcenter"><img src="https://img.shields.io/static/v1?label=&message=Research+Center&color=blueviolet&style=for-the-badge&logo=internetarchive" alt="Research Center"></a>
  <a href="https://www.paypal.com/donate/?hosted_button_id=3HC4L477XZRXU"><img src="https://img.shields.io/static/v1?label=Support%3A&message=Donate&color=green&style=for-the-badge&logo=paypal" alt="Support: - Donate"></a>
  <br/>
  <br/>
</div>

https://github.com/TheItCrOw/Bundestags-Mine/assets/49918134/4a999f86-10ce-4be8-a306-5506e7e0a830

<sub><i>(German video)</i></sub>


## About
![image](https://github.com/TheItCrOw/Bundestags-Mine/assets/49918134/1faf56c0-5ed9-4263-af21-37a957fea925)
Bundestags-Mine is an environment for evaluating German government documents by means of various Natural Language Processing techniques and visualizing them via a publicly accessable and intuitive web application as well as providing the resulting data for download.
Within this environment, we processes the following types of government data, which are then visualized within the web application for a platform-independent and responsive interface: 
- Minutes of plenary proceedings
- Agenda Items
- Polls

We gather this data from the offical Bundestag Data Service, apply various NLP/AI techniques onto it and make these results available on the website.

## Usage
Dive into several features of the [web interface](https://bundestag-mine.de/) to interact with the Bundestag like never before.

Read the newspaper: "Neues vom Schürfer"             |  Explore all speeches
:-------------------------:|:-------------------------:
![image](https://user-images.githubusercontent.com/49918134/226877498-9f773b30-3ad9-4e3b-b2cd-383cc3533575.png)  |  ![image](https://user-images.githubusercontent.com/49918134/182587206-f30e256c-2bc3-490b-9dbf-8d9ebdcd3801.png)
Analyse specific topics             |  Browse through topics
![image](https://user-images.githubusercontent.com/49918134/182587945-7f722350-1100-4065-84ab-32ed965c15a3.png) | ![image](https://user-images.githubusercontent.com/49918134/182587979-5e6bca81-644f-49eb-9be2-98ff5bf2a8cb.png)

## Natural Language Processing

Within the Bundestags-Mine, we use a list of NLP techniques through pipelines to preprocess and analyse the governmental documents:
- Tokenziation
- Lemmatization
- POS-Tagging
- Named-Entity-Recognition
- Sentiment Analysis
- Automatic Text Summarization
- Translation

# Citation

```
@inproceedings{Boenisch:et:al:2023,
  title     = {{Bundestags-Mine}: Natural Language Processing for Extracting
               Key Information from Government Documents},
  isbn      = {9781643684734},
  issn      = {1879-8314},
  url       = {http://dx.doi.org/10.3233/FAIA230996},
  doi       = {10.3233/faia230996},
  booktitle = {Legal Knowledge and Information Systems},
  publisher = {IOS Press},
  author    = {B\"{o}nisch, Kevin and Abrami, Giuseppe and Wehnert, Sabine and Mehler, Alexander},
  year      = {2023}
}
```

<i>The majority of the NLP pipelines are provided by the TextImager made by the Text Technology Lab</i> ([pdf](https://aclanthology.org/C16-2013.pdf) [bibtex](https://aclanthology.org/C16-2013.bib) [github](https://github.com/texttechnologylab/textimager-uima))

