
var lightbox=document.createElement('div');
lightbox.visible=false;
lightbox.focusOnShow="";
lightbox.onclick=function(e){e = e || window.event;var target = e.target || e.srcElement;if(target==lightbox){e.stopPropagation();lightbox.hide();}};
lightbox.hide=function(){document.body.removeChild(lightbox);lightbox.visible=false;};
lightbox.show=function(){
	lightbox.setAttribute("style","position:fixed;top:0;width: 100%;background-color: rgba(0,0,0,0.5);height: 100%;z-index: 1000;left: 0px;padding-top:"+window.innerHeight/2+"px;");
	document.body.appendChild(lightbox);
	lightbox.visible=true;
	lightbox.focusField();
};
//set the lightbox padding when window is resized
window.onresize=function(){lightbox.style.paddingTop=window.innerHeight/2+"px";};
lightbox.setContent=function(ccontent){
	lightbox.innerHTML="<div style='background-color:white;width:550px;margin:auto;padding:32px;padding-top:10px;'></div>";lightbox.firstChild.appendChild(ccontent);setTimeout(function(){lightbox.firstChild.style.marginTop="-"+(lightbox.firstChild.firstChild.clientHeight+64)/2+"px";},1);
	lightbox.focusField();
	return lightbox;
};
lightbox.setFocus=function(elemId){
	if(lightbox.visible){
		lightbox.focusField();
	}else{
		lightbox.focusOnShow=elemId;
	}
	return lightbox;
};
lightbox.focusField=function(){
	if(lightbox.visible && lightbox.focusOnShow!="" && lightbox.focusOnShow!=null){
		try{document.getElementById(lightbox.focusOnShow).focus();}
		catch(e){}
	}
};

/*
lightbox.setClosemethod(function)
lightbox.close->closes the lightbox
lightbox.newlightbox(content, params->{closeMethod:function,onshow}).show().hide()

*/

var Lightbox2 = function () {
	'use strict';
	function lightboxBuilder(content) {
		var lb = document.createElement('div');
		lb.innerHTML = "<div style='background-color:white;width:550px;margin:auto;padding:32px;padding-top:10px;'><div></div></div>"; lb.firstChild.appendChild(content); setTimeout(function () { lb.firstChild.style.marginTop = "-" + (lb.firstChild.firstChild.clientHeight + 64) / 2 + "px"; }, 1);

		lb.hide = function () { document.body.removeChild(lb); };

		lb.getTopBar = function () {
			return lb.firstChild.firstChild;
		};

		function onShow() { }

		function closeMethod() {
			this.onclick = function (e) { e = e || window.event; var target = e.target || e.srcElement; if (target == this) { e.stopPropagation(); this.hide(); } };
		}

		var builder = {
			build: function () {
				lb.setAttribute("style", "position:fixed;top:0;width: 100%;background-color: rgba(0,0,0,0.5);height: 100%;z-index: 1000;left: 0px;padding-top:" + window.innerHeight / 2 + "px;");
				document.body.appendChild(lb);
				closeMethod.apply(lb);
				//this.onShow();
				return lb;
			},
			setCloseMethod: function (method) {
				closeMethod = method;
				return this;
			}
		};
		return builder;
		
	}
	return { builder: lightboxBuilder }

}
	
