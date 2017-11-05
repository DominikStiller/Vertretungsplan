// Check if we are on a VP page
if (typeof page !== 'undefined' && (page == "students" || page == "teachers")) {
   // var page, currentDate, dates are declared in the html
   var ajaxBase = "/ajax/" + page;
   var cache = {};

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
      if (date in cache)
         $("main").html(cache[date]);
   }
}

console.log("Source code: https://github.com/DominikStiller/Vertretungsplan");

if (document.cookie.indexOf("ga_optout=true") > -1) {
   window["ga-disable-UA-99091816-1"] = true;
}
