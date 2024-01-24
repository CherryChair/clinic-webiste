import axios from 'axios';
import Cookies from 'js-cookie';
import 'jwt-decode'

const decodeTokenAndSetRole = (token) => {
    let decodedToken = jwt_decode(token);
    decodedToken.IsPatient ? localStorage.setItem("isPatient", true) : localStorage.setItem("isPatient", false);
    decodedToken.IsDoctor ? localStorage.setItem("isDoctor", true) : localStorage.setItem("isDoctor", false);
    decodedToken.IsAdmin ? localStorage.setItem("isAdmin", true) : localStorage.setItem("isAdmin", false);
}

export const isPatient = () => {
    return localStorage.getItem("isPatient");
}

export const isDoctor = () => {
    return localStorage.getItem("isDoctor");
}

export const isAdmin = () => {
    return localStorage.getItem("isAdmin");
}

export const setAuthToken = token => {
   if (token) {
       axios.defaults.headers.common["Authorization"] = `Bearer ${token}`;
   }
   else
       delete axios.defaults.headers.common["Authorization"];
}

const handleSubmit = event => {
    event.preventDefault();
    //reqres registered sample user
    const loginPayload = {
      email: 'mkwl1@wp.pl',
      password: 'Root123@'
    }
    axios.post("https://localhost:7298/Patients/login", loginPayload)
      .then(response => {
        //get token from response
        const token  =  response.data.token;

        decodeTokenAndSetRole(token);
  
        //set JWT token to local
        Cookies.set("token", token, {expires: Date.parse(response.data.expiration)});
        //set token to axios common header
        setAuthToken(token);
  
 //redirect user to home page
        window.location.href = '/'
      })
      .catch(err => console.log(err));
    };
 


function LoginPage() {
    return (
      <div>
          <form onSubmit={handleSubmit} >
            <button type="submit"> Login</button>
          </form>
      </div>
    );
 }
 
 export default LoginPage;