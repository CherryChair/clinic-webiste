import axios from 'axios';
import Cookies from 'js-cookie';
import { useState } from 'react';
import ErrorBox from '../components/ErrorBox';
import { decodeTokenAndSetRole, setAuthToken } from './Login';
import FormField from '../components/FormField';


function RegisterPage() {
    const [wrongCredentials, setWrongCredentials] = useState(false);
    const [errorMsg, setErrorMsg] = useState("");
    function handleSubmit(event) {
        event.preventDefault();
        if (event.target.password.value !== event.target.confirmedPassword.value) {
          setWrongCredentials(true);
          setErrorMsg("Passwords don't match");
          return;
        }
        const registerPayload = {
          firstName: event.target.firstName.value,
          surname: event.target.surname.value,
          email: event.target.email.value,
          password: event.target.password.value,
        }
        axios.post("/Patients/register", registerPayload)
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
          .catch(err => {
            console.log(err);
            setWrongCredentials(true);
            setErrorMsg("Server Error");
        });
    };

    function changeErrorFlag() {
        wrongCredentials ? setWrongCredentials(false) : setWrongCredentials(true);
        if (wrongCredentials) {
          setErrorMsg("");
        }
    }
    
    return (
        <>
          <div className="flex min-h-full flex-1 flex-col justify-center px-6 py-12 lg:px-8">
            <div className="sm:mx-auto sm:w-full sm:max-w-sm">
              <img
                className="mx-auto h-14 w-auto"
                src="/logo.png"
                alt="Your Company"
              />
              <h2 className="mt-6 text-center text-2xl font-bold leading-9 tracking-tight text-gray-900">
                Register as patient
              </h2>
            </div>
    
            <div className="mt-10 sm:mx-auto sm:w-full sm:max-w-sm">
              {/* <form className="space-y-6" action="#" method="POST"> */}
              <form className="space-y-6" onSubmit={handleSubmit}>
                <FormField type="text" attr="firstName" label="First name"/>
                <FormField type="text" attr="surname" label="Surname"/>
                <FormField type="email" attr="email" label="Email address"/>
                <FormField type="password" attr="password" label="Password" 
                  className={errorMsg === "Passwords don't match" ? " border-red-500" : ""}
                />
                <FormField type="password" attr="confirmedPassword" label="Confirm password" 
                  className={errorMsg === "Passwords don't match" ? " border-red-500" : ""}
                />
                <ErrorBox errorFlag={wrongCredentials} changeErrorFlag={changeErrorFlag} errorMsg={errorMsg}/>
                <div>
                  <button
                    type="submit"
                    className="flex w-full justify-center rounded-md bg-indigo-600 px-3 py-1.5 text-sm font-semibold leading-6 text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                  >
                    Register
                  </button>
                </div>
              </form>
            </div>
          </div>
        </>
      )
 }
 
 export default RegisterPage;