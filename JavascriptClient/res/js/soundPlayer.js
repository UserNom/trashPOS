//http://www.storiesinflight.com/html5/audio.html
var SoundPlayer = {
	sounds: {},
	add: function (name, url) {
		this.sounds[name] = new Audio(url);
	},
	play: function (name) {
		for (var a = 0; a < this.audiochannels.length; a++) {
			var thistime = new Date();
			if (this.audiochannels[a]['finished'] < thistime.getTime()) {			// is this channel finished?
				this.audiochannels[a]['finished'] = thistime.getTime() + this.sounds[name].duration * 1000;
				//this.audiochannels[a]['channel'].src = this.sounds[name].src;
				//this.audiochannels[a]['channel'].load();
				this.audiochannels[a]['channel'] = this.sounds[name].cloneNode();
				this.audiochannels[a]['channel'].play();
				break;
			}
		}
	},
	audiochannels: (function () {
		var x = [];
		for (var a = 0; a < 10; a++) {	// prepare the channels
			x[a] = new Array();
			x[a]['channel'] = new Audio();	// create a new audio object
			x[a]['finished'] = -1;	// expected end time for this channel
		}
		return x;
	}())
}