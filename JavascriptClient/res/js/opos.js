var oposPrintReceipt = null;


var scannerList=[];
var printerList=[];


function showOposConfig() {
	W3Lightbox.build({
		title: 'Point of sale devices config',
		content: oposConfig,
		buttons: [
			{
				label: 'Refresh devices',
				onclick: function () { oposConfig.refresh() }
			},
			{
				label: 'Update',
				onclick: function () {
					$.connection.posHub.server.claimScanner(getSelectedScanner());
					$.connection.posHub.server.claimPrinter(getSelectedPrinter());
				}
			}
		]
	}).show();//setContent(oposConfig).show();
}
var oposConfig;

function updateDeviceSelector(selectElem, deviceList) {
	for (var i = 0; i < deviceList.length; i++) {
		var d = deviceList[i];
		var text = (d.LogicalNames[0] || d.Description || ((d.ManufacturerName || d.ServiceObjectName) + " " + d.Type));
		if (selectElem.options[i + 1] != undefined) {
			selectElem.options[i + 1].text = text;
			selectElem.options[i + 1].value = i;
		} else {
			var op = document.createElement('option');
			op.text = text;
			op.value = i;
			selectElem.appendChild(op);
		}
	}
	for (var i = deviceList.length + 1; selectElem.options.length > i; selectElem.options[i].remove());
}

(function () {
	oposConfig = document.createElement('form');

	oposConfig.printerSelect = document.createElement('select');
	oposConfig.printerSelect.innerHTML = '<option value="none">None</option>';
	oposConfig.appendChild(oposConfig.printerSelect);

	oposConfig.scannerSelect = document.createElement('select');
	oposConfig.scannerSelect.innerHTML = '<option value="none">None</option>';
	oposConfig.appendChild(oposConfig.scannerSelect);

	oposConfig.appendChild(document.createElement('br'));
	oposConfig.appendChild(document.createElement('br'));

	window.getSelectedPrinter = function () {
		var selected = oposConfig.printerSelect.selectedIndex;
		return (selected <= 0) ? null : printerList[oposConfig.printerSelect[selected].value];
	}
	window.getSelectedScanner = function () {
		var selected = oposConfig.scannerSelect.selectedIndex;
		return (selected <= 0) ? null : scannerList[oposConfig.scannerSelect[selected].value];
	}
	oposConfig.onsubmit = function (e) {
		$.connection.posHub.server.claimScanner(getSelectedScanner());
		$.connection.posHub.server.claimPrinter(getSelectedPrinter());
		return false;
	}
	oposConfig.refresh = function () {
		updateDeviceSelector(oposConfig.printerSelect, printerList);
		updateDeviceSelector(oposConfig.scannerSelect, scannerList);
		return false;
	};
}());

$(function () {
	//Set the hubs URL for the connection
	$.connection.hub.url = "http://localhost:8080/signalr";
	// Declare a proxy to reference the hub.
	var chat = $.connection.posHub;
	// Create a function that the hub can call to broadcast messages.
	chat.client.broadcastMessage = function (name, message) {
		// Html encode display name and message.
		console.log(name + ': ' + message);
	};

	chat.client.broadcastAlert = function (message) {
		alert(message);
	};

	//Receive a barcode read from server
	chat.client.barcodeRead = function (barcode) {
		if (!scannedField.disabled) {
			scannedField.value = barcode;
			scanForm.onsubmit();
		} else {
			SoundPlayer.play('itemScanned');
			//sounds.itemScanned.play();
		}
	};

	chat.client.updateScannerList = function (list, isSelectedScannerGone) {
		if (isSelectedScannerGone) {
			alert("Scanner disconnected, select another one.");
			oposConfig.scannerSelect.selectedIndex = 0;
		}
		scannerList = list;
		oposConfig.refresh();
	};

	chat.client.updatePrinterList = function (list, isSelectedPrinterGone) {
		if (isSelectedPrinterGone) {
			alert("Printer disconnected, select another one.");
			oposConfig.printerSelect.selectedIndex = 0;
		}
		printerList = list;
		oposConfig.refresh();
	};

	// http://stackoverflow.com/questions/19688673/signalr-there-was-an-error-invoking-hub-method-xxx
	$.connection.hub.error(function (error) {
		console.log('SignalR error: ' + error)
	});

	// Start the connection.
	$.connection.hub.start({ transport: ["webSockets", "serverSentEvents", "foreverFrame", "longPolling"] }).done(function () {
		//chat.server.getStatus();
	});

	//detect connection changes
	$.connection.hub.stateChanged(function (state) {
		console.log(state);
		//if(state.newState==$.signalR.connectionState.connected)
		switch (state.newState) {
			case $.signalR.connectionState.connected:
				document.getElementById('connectionStatus').innerHTML = 'Connected(POS)<br><a href="#" onclick="showOposConfig()">Config POS</a>';
				break;
			case $.signalR.connectionState.connecting:
				document.getElementById('connectionStatus').innerHTML = "Connecting";
				break;
			case $.signalR.connectionState.disconnected:
				printerList = [];
				scannerList = [];
				oposConfig.refresh();
				var connectionStatusElem = document.getElementById('connectionStatus');
				connectionStatusElem.innerHTML = "";
				if ($.connection.hub.lastError){ connectionStatusElem.innerHTML += $.connection.hub.lastError.message + '<br>'; }
				connectionStatusElem.innerHTML += "Click to try reconnecting";
				connectionStatusElem.onclick = function () {
					$.connection.hub.start();
					this.onclick = null;
				};
				break;
			case $.signalR.connectionState.reconnecting:
				document.getElementById('connectionStatus').innerHTML = "Reconnecting(POS)";
				break;
		}
	});
	


	oposPrintReceipt= function() {
		var receiptText = "";
		for (var k in order.items) {
			var currItem = order.items[k];
			receiptText += (currItem.item.name || "ItemCode: " + currItem.item.upc) + "\r\n    X " + currItem.qty + "    @ $" + currItem.item.price + "    $" + (currItem.item.price * currItem.qty).toFixed(2) + "\r\n";
		}
		if (order.backorderedItems.length > 0) {
			receiptText += "\r\n" +
				"***Backordered Items***\r\n" +
				"    Delivery:" + order.shipMethod + "\r\n\r\n";
		}
		for (var k in order.backorderedItems) {
			var currItem = order.backorderedItems[k];
			receiptText += (currItem.item.name || "ItemCode: " + currItem.item.upc) + "\r\n    X " + currItem.qty + "    @ $" + currItem.item.price + "    $" + (currItem.item.price * currItem.qty).toFixed(2) + "\r\n";
		}
		receiptText += "\r\n\r\n";
		var ordertotal = order.getOrderTotal();
		receiptText += "            Subtotal:     " + ordertotal.subtotal.toFixed(2)+"\r\n";
		receiptText += "            Shipping:     " + ordertotal.shipping.toFixed(2) + "\r\n";
		receiptText += "                Fees:     " + ordertotal.fees.toFixed(2) + "\r\n";
		receiptText += "               Taxes:     " + (ordertotal.tax1 + ordertotal.tax2).toFixed(2) + "\r\n";
		receiptText += "               Total:     " + ordertotal.total.toFixed(2) + "\r\n";
		chat.server.printReceipt(receiptText,order.orderNo);
	}

});