/**
*
* Making sure Object.assign works
*
*/
//https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/assign#Polyfill
if (!Object.assign) {
	Object.defineProperty(Object, 'assign', {
		enumerable: false,
		configurable: true,
		writable: true,
		value: function (target, firstSource) {
			'use strict';
			if (target === undefined || target === null) {
				throw new TypeError('Cannot convert first argument to object');
			}

			var to = Object(target);
			for (var i = 1; i < arguments.length; i++) {
				var nextSource = arguments[i];
				if (nextSource === undefined || nextSource === null) {
					continue;
				}
				nextSource = Object(nextSource);

				var keysArray = Object.keys(Object(nextSource));
				for (var nextIndex = 0, len = keysArray.length; nextIndex < len; nextIndex++) {
					var nextKey = keysArray[nextIndex];
					var desc = Object.getOwnPropertyDescriptor(nextSource, nextKey);
					if (desc !== undefined && desc.enumerable) {
						to[nextKey] = nextSource[nextKey];
					}
				}
			}
			return to;
		}
	});
}




W3Lightbox = function () {
	"use strict";
	var Lightbox = {
		_lightboxList: [],
		_lightboxEscHandler: function (e) {
			//might not work in all browsers, deal with it.
			if ((e.key.toLowerCase() == "esc" || e.key.toLowerCase() == "escape")
					&& Lightbox._lightboxList.length > 0
					&& Lightbox._lightboxList[Lightbox._lightboxList.length - 1].cancelOnEsc) {
				Lightbox._lightboxList[Lightbox._lightboxList.length - 1].cancel();
				e.stopPropagation();
			}
			e.cancelBubble = true;
		},
		lightboxCreator: function (options) {
			var wrapper = {};
			wrapper.dom = document.createElement('div');
			wrapper.dom.classList.add('lightbox_wrapper');
			wrapper.close = function () {
				if (config.onclose()) {
					document.body.removeChild(wrapper.dom);
					Lightbox._lightboxList.pop();
					return true;
				}
				return false;
			};
			wrapper.cancel = function () {
				if (config.oncancel()) {
					document.body.removeChild(wrapper.dom);
					Lightbox._lightboxList.pop();
					return true;
				}
				return false;
			};
			wrapper.show = function () {
				if (config.onshow()) {
					document.body.appendChild(wrapper.dom);
					Lightbox._lightboxList.push(wrapper);
					return true;
				}
				return false;
			}

			var config = {
				content: '...',
				title: '',
				//buttons:[{label:'OK',color:'#000',bgcolor:'#dedede',onclick:function(){wrapper.close()}}],
				buttons: [],
				cancelOnEsc: true,
				cancelOnBgClick: true,
				closeButton: true,
				onclose: function () { return true; },
				oncancel: function () { return true; },
				onshow: function () { return true },
				width: 550
			};
			Object.assign(config, options);

			wrapper.dom.innerHTML = '<div class="lightbox_box" style="background-color:white;width:' + config.width + 'px;margin:auto;">'
				+ ((config.closeButton || config.title) ? '<div class="lightbox_topBar">' + config.title + '</div>' : '')
				+ '<div class="lightbox_content"></div>'
				+ ((config.buttons.length > 0) ? '<div class="lightbox_bottomBar"></div>' : '')
				+ '</div>';
			wrapper.dom.getElementsByClassName('lightbox_content')[0].appendChild(config.content);
			wrapper.dom.setAttribute("style", "padding-top:" + window.innerHeight / 2.4 + "px;");

			if (config.closeButton) {
				var closeButton = document.createElement('button');
				closeButton.innerHTML = "X";
				closeButton.onclick = wrapper.cancel;
				closeButton.classList.add('lightbox_closeButton');
				closeButton.classList.add('lightbox_button');
				wrapper.dom.getElementsByClassName('lightbox_topBar')[0].appendChild(closeButton);
			}

			wrapper.dom.addEventListener('submit', function (e) {
				if (!e.defaultPrevented) {
					//form submit returned true - nothing left to do here, cancel lightbox
					wrapper.cancel();
				}
			}, false);

			for (var b in config.buttons) {
				var tempbutton = document.createElement('button');
				tempbutton.innerHTML = config.buttons[b].label;
				Object.assign(tempbutton.style, config.buttons[b].style);
				//tempbutton.style.color = config.buttons[b].color || '#000';
				//tempbutton.style.backgroundColor = config.buttons[b].bgcolor || '#dedede';
				tempbutton.onclick = config.buttons[b].onclick;
				tempbutton.classList.add('lightbox_button');
				tempbutton.lightbox = wrapper;
				wrapper.dom.getElementsByClassName('lightbox_bottomBar')[0].appendChild(tempbutton);
			}

			if (config.cancelOnBgClick) {
				wrapper.dom.onclick = function (e) { e = e || window.event; var target = e.target || e.srcElement; if (target == wrapper.dom) { e.stopPropagation(); wrapper.cancel(); } };
			}

			wrapper.cancelOnEsc = config.cancelOnEsc;

			setTimeout(
				function () { wrapper.dom.firstChild.style.marginTop = "-" + (wrapper.dom.firstChild.clientHeight/*.firstChild.clientHeight*/) / 2.4 + "px"; },
				1
			);

			return wrapper;

		}
	}
	document.addEventListener('keypress', Lightbox._lightboxEscHandler, false);
	window.addEventListener('resize', function () {
		for (var l in Lightbox._lightboxList) {
			Lightbox._lightboxList[l].dom.style.paddingTop = window.innerHeight / 2.4 + "px";
		}
	}, false);

	return { build: Lightbox.lightboxCreator };
}();


