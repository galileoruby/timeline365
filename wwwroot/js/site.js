document.addEventListener("DOMContentLoaded", function () {
	var forms = document.querySelectorAll("form[data-auto-submit='true']");

	forms.forEach(function (form) {
		var selects = form.querySelectorAll("select");
		selects.forEach(function (item) {
			item.addEventListener("change", function () {
				form.submit();
			});
		});
	});
});
