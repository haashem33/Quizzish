const minnav = document.querySelector(".navbara");
const navknapp = document.querySelector(".mobil-meny");
const lome = document.querySelector(".lome");
const hem = document.querySelector(".hem");
navknapp.addEventListener("click", () => {
  const visas = minnav.getAttribute("data-visible");

  if (visas == "false") {
    minnav.setAttribute("data-visible", true);
    navknapp.setAttribute("aria-expanded", true);
    lome.style.position = "fixed";
    lome.style.top = "0";
    lome.style.zIndex = "100";
    navknapp.classList.toggle("open");
    hem.style.boxShadow = "inset 0px 1000px 500px rgba(0, 0, 0, 0.15)";
    document.body.style.overflowY = "Hidden";
  } else if (visas == "true") {
    minnav.setAttribute("data-visible", false);
    navknapp.setAttribute("aria-expanded", false);
    lome.style.position = "static";
    lome.style.top = "0";
    lome.style.zIndex = "100";
    navknapp.classList.toggle("open");
    document.body.style.overflowY = "scroll";
    hem.style.boxShadow = "inset 0px 1000px 500px rgba(0, 0, 0, 0.35)";
  }
});

$(window).on("load", function () {
  $(".minanimation").fadeOut(2000);
  $(".minnya").fadeIn(2000);
});




$(".button-68").click(function () {
    checked = $("input[type=checkbox]:checked").length;

    if (!checked) {
        event.preventDefault();
        alert("Du måste välja en katogori");
            
    }
    else {
        $(".star").css("display", "none");

        event.preventDefault();
        
        $.ajax({
            url: "/Home/quiz2",
            type: "POST",
            data: $("form").serialize(),
            success: function (result) {
                $(".frogor").html(result)

            }, error: function (ajaxContext) {
                alert(ajaxContext.responseText)
            }
        });
    }


});

