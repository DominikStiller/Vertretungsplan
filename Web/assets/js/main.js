// var type, currentDate, dates are declared in the html
var ajaxBase = "/ajax/" + type;
var cache = {};

console.log("Source code: https://github.com/DominikStiller/Vertretungsplan");

// Warm up cache
dates.forEach(function(date) {
   $.get(ajaxBase + "/" + date, function(data) {
      cache[date] = data;
   });
});

$(function() {
   cache[currentDate] = $("main").html();

   $("body").on("click", "#previousdate, #nextdate", function() {
      displayDate($(this).data("date"));
   });
   $("body").on("change", "#dateselector", function() {
      displayDate(this.value);
   });
});

function displayDate(date) {
   // Cache misses are very unlikely to occur, since all html is loaded at the same time
   // This means there should be no references to dates that are not cached
   // To show new dates the user can refresh the page
   $("main").html(cache[date]);
}

if (document.cookie.indexOf("ga_optout=true") > -1) {
   window["ga-disable-UA-99091816-1"] = true;
}
