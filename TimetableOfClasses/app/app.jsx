var ReactDOM = require('react-dom');
var React = require('react');
//var ItemsList = require('./components/ItemsList.jsx');
var LoginForm = require('./components/LoginForm.jsx');
 
// const propsValues = {
//     title: "Список смартфонов",
//     items: [
//         "HTC U Ultra", 
//         "iPhone 7", 
//         "Google Pixel", 
//         "Huawei P9", 
//         "Meizu Pro 6", 
// 		"Asus Zenfone 3",
// 		"SUNGYONG2456"
//     ]
// };
 
ReactDOM.render(
	<div>
	<LoginForm>
	</LoginForm>    
	</div>,
    document.getElementById("app")
)