// var dates, type are declared in the html
var ajaxBase = "/ajax/" + type;
var cache = {};

$(function() {
   console.log("Source code: https://github.com/DominikStiller/Vertretungsplan");

   // Warm up cache
   dates.forEach(function(date) {
      $.get(ajaxBase + "/" + date, function(data) {
         cache[date] = data;
      });
   });

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
