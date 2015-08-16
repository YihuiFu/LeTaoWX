$(document).ready(function () {

	// 字体自动变化
	function  responseFonstSize(step) {
       var deviceWidth = window.innerWidth / step;
       var htmlFont = document.getElementsByTagName('html');
       htmlFont[0].style.fontSize = deviceWidth + 'px';
    }responseFonstSize(8);

    $(window).resize(function () {
    	responseFonstSize(8);
	})

    $("#tipclose").click(function(){
	  $(".in_tip").hide();
    });
	
	
	<!--弹窗的提示框-->
	$('.cd-popup-trigger').on('click', function(event){
		$('.tempmyshow').addClass('is-visible');
	});
	//close popup
	$('.tempmyshow').on('click', function(event){
		if( $(event.target).is('#cd-popup-close') || $(event.target).is('.tempmyshow') ) {
			$(this).removeClass('is-visible');
		}
	});
	
	//close adimg
	$(".ad_colse").click(function(){
		$(".adimg").hide();
	});	
	
	<!--弹窗-->
	$('.cd-popup-trigger1').click(function () {
		$('.tempmyshow1').addClass('is-visible');	
	})
	$('.cd-popup-close1').click(function () {
		$('.tempmyshow1').removeClass('is-visible');	
	})
	
	//人才加盟
	  $(".MenuCard").bind("click",function(){
		  $(".MenuCard").removeClass("active");
		  $(this).addClass("active");
		  $(".contactCard").hide();
		  $(this).next().slideDown(600);
	  })
	
	//定制成功
	function favhide () {
	  $('.plan_tip').hide();
	}favhide ();
	$('.plan_click').on('click',function () {
	  $('.plan_tip').show();
	  timer = setTimeout(favhide,30000);
	})
	

	$('.cd-popup-trigger').on('click', function(event){
		$('.tempmyshow1').addClass('is-visible');
	});
	//close popup
	$('.tempmyshow1').on('click', function(event){
		if( $(event.target).is('#cd-popup-close') || $(event.target).is('.tempmyshow1') ) {
			$(this).removeClass('is-visible');
		}
	});




})


