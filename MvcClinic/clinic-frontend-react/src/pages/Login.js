import axios from 'axios';
import Cookies from 'js-cookie';
import { jwtDecode } from 'jwt-decode';
import { useState } from 'react';
import ErrorBox from '../components/ErrorBox';
import FormField from '../components/FormField';
import ButtonAccept from '../components/ButtonAccept';

export const decodeTokenAndSetRole = (token) => {
    let decodedToken = jwtDecode(token);
    decodedToken.IsPatient ? localStorage.setItem("isPatient", true) : localStorage.setItem("isPatient", false);
    decodedToken.IsDoctor ? localStorage.setItem("isDoctor", true) : localStorage.setItem("isDoctor", false);
    decodedToken.IsAdmin ? localStorage.setItem("isAdmin", true) : localStorage.setItem("isAdmin", false);
}

export const isPatient = () => {
    let item = localStorage.getItem("isPatient");
    if (item && item === "true") {
        return true;
    }
    return false;
}

export const isDoctor = () => {
    let item = localStorage.getItem("isDoctor");
    if (item && item === "true") {
        return true;
    }
    return false;
}

export const isAdmin = () => {
    let item = localStorage.getItem("isAdmin");
    if (item && item === "true") {
        return true;
    }
    return false;
}

export const isLoggedIn = () => {
    return Cookies.get("token") ? true : false;
}

export const setAuthToken = token => {
   if (token) {
       axios.defaults.headers.common["Authorization"] = `Bearer ${token}`;
   }
   else
       delete axios.defaults.headers.common["Authorization"];
}


function LoginPage() {
    const [wrongCredentials, setWrongCredentials] = useState(false);
    function handleSubmit(event) {
        event.preventDefault();
        const loginPayload = {
          email: event.target.email.value,
          password: event.target.password.value
        }
        axios.post("/login", loginPayload)
          .then(response => {
            const token  =  response.data.token;
    
            decodeTokenAndSetRole(token);
      
            Cookies.set("token", token, {expires: Date.parse(response.data.expiration)});
            setAuthToken(token);
      
            window.location.href = '/'
          })
          .catch(err => {
            console.log(err);
            setWrongCredentials(true);
        });
    };

    function changeErrorFlag() {
        wrongCredentials ? setWrongCredentials(false) : setWrongCredentials(true);
    }
    
    return (
        <>
          <div className="flex min-h-full flex-1 flex-col justify-center px-6 py-12 lg:px-8">
            <div className="sm:mx-auto sm:w-full sm:max-w-sm">
              <img
                className="mx-auto h-10 w-auto"
                src="https://tailwindui.com/img/logos/mark.svg?color=indigo&shade=600"
                alt="Your Company"
              />
              <h2 className="mt-10 text-center text-2xl font-bold leading-9 tracking-tight text-gray-900">
                Sign in to your account
              </h2>
            </div>
    
            <div className="mt-10 sm:mx-auto sm:w-full sm:max-w-sm">
              <form className="space-y-6" onSubmit={handleSubmit}>
                <FormField type="email" attr="email" label="Email address"/>
                <FormField type="password" attr="password" label="Password"/>
                <ErrorBox errorFlag={wrongCredentials} changeErrorFlag={changeErrorFlag} errorMsg={"Wrong credentials"}/>
                <div>
                  <ButtonAccept type="submit" text="Sign in"/>
                </div>
              </form>
    
              <p className="mt-10 text-center text-sm text-gray-500">
                Not a member?{' '}
                <a href="/register" className="font-semibold leading-6 text-indigo-600 hover:text-indigo-500">
                  Register
                </a>
              </p>
            </div>
          </div>
        </>
      )
 }
 
 export default LoginPage;