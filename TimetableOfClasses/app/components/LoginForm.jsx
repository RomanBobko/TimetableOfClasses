var React = require('react');

class LoginForm extends React.Component {
    constructor(props){
        super(props);
		this.loginError = '';        
		this.userName = '';
	}    
	
	login(){
		console.log('');
	}
               
    render() {
        return(						
            <form>
				<h2>Выполните вход</h2>
                {this.loginError && 
				<div className="alert alert-danger" role="alert">{this.loginError}</div>
				}
                <div className="form-group">
                    <label>Имя пользователя:</label>
                    <input className="form-control" type="text" />
                </div>
                <div className="form-group">
                    <label>Пароль:</label>
                    <input className="form-control" type="password" />
                </div>
                <input className="btn btn-primary" type="submit" value="Войти" />
            </form>
            );
    }
}
 
module.exports = LoginForm;
