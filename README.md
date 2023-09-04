# Docker installation

1) Create a /opt/data folder, then download content from https://github.com/Necksus/TradingData/tree/main/Data inside
2) Create a docker-compose.yml file (change the volume and port as needed):

```
version: '3.8'

services:
    sharpitpm:
        container_name: sharpitpm_live
        image: "necksus/sharpitpm:latest"
        volumes:
          - "/opt/data:/data"
        ports:
          - "1337:80"
```

**Important:** use necksus/sharpitpm.arm for arm host (such as Raspberry Pi)

3) Execute the command `docker-compose up -d` to create the container.
4) Navigate to [http://hostname:1337/swagger](http://hostname:1337/swagger).


# How to add new ticker?

Every ticker should have an entry in the JSON file /data/NewsEvents/DataSources.json with:

* Ticker (mandatory): the stock ticker
* NewsUrls (mandatory): define how to grab press releases

  * Urls (mandatory): list of the investor website urls
  * DateFormat (optional): when the date format cannot be parsed by default configuration, ask to Chat GPT the question '*what is the c# format to parse "Aug 24, 2023" using DateTime.TryParseExact?*' (1), and copy the response.
  * PagerDefinition (optional):
    * NextButton: custom XPath to find the "Next" button, when it's not detected automatically.
      1. Open the Url with Chrome
      2. Type "F12" to open the developer toolbar
      3. Use the arrow to select the next button htlm element
      4. Ask Chat GPT the question 'what is the xpath to select this html tag : "<a class="page-link" href="https://ir.clearme.com/news-events/press-releases?page=2" aria-label="Next Page<a class="page-link" 
         href="https://ir.clearme.com/news-events/press-releases?page=2" 
         aria-label="Next Page">" inside a whole html document?'
* EventsUrls (optional): define how to grab events


**Important:**

(1): Sometimes the date ends with 2 or 3 letters with the timezone code (for example "Aug 29, 2023 6:30 am EDT"). Remove these letters from the Chat GPT question ("Aug 29, 2023 6:30 am" in sample above).

(2) Time format can be different between press releases and events web page.
