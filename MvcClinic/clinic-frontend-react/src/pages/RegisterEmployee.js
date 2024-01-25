import axios from 'axios';
import Cookies from 'js-cookie';
import { useState } from 'react';
import ErrorBox from '../components/ErrorBox';
import { decodeTokenAndSetRole, setAuthToken } from './Login';


function RegisterPage() {
    const [wrongCredentials, setWrongCredentials] = useState(false);
    const [errorMsg, setErrorMsg] = useState("");
    const passwordClassName = "block w-full rounded-md border-1 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6";
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
        axios.post("https://localhost:7298/Patients/register", registerPayload)
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
                className="mx-auto h-10 w-auto"
                src="https://tailwindui.com/img/logos/mark.svg?color=indigo&shade=600"
                alt="Your Company"
              />
              <h2 className="mt-10 text-center text-2xl font-bold leading-9 tracking-tight text-gray-900">
                Register as patient
              </h2>
            </div>
    
            <div className="mt-10 sm:mx-auto sm:w-full sm:max-w-sm">
              {/* <form className="space-y-6" action="#" method="POST"> */}
              <form className="space-y-6" onSubmit={handleSubmit}>
                <div>
                  <div className="flex items-center justify-between">
                    <label htmlFor="firstName" className="block text-sm font-medium leading-6 text-gray-900">
                        First name
                    </label>
                  </div>
                  <div className="mt-2">
                    <input
                      id="firstName"
                      name="firstName"
                      type="text"
                      autoComplete="firstName"
                      required
                      className="block w-full rounded-md border-1 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6"
                    />
                  </div>
                </div>

                <div>
                  <div className="flex items-center justify-between">
                    <label htmlFor="surname" className="block text-sm font-medium leading-6 text-gray-900">
                        Surname
                    </label>
                  </div>
                  <div className="mt-2">
                    <input
                      id="surname"
                      name="surname"
                      type="text"
                      autoComplete="surname"
                      required
                      className="block w-full rounded-md border-1 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6"
                    />
                  </div>
                </div>

                <div>
                  <div className="flex items-center justify-between">
                    <label htmlFor="email" className="block text-sm font-medium leading-6 text-gray-900">
                        Email address
                    </label>
                  </div>
                  <div className="mt-2">
                    <input
                      id="email"
                      name="email"
                      type="email"
                      autoComplete="email"
                      required
                      className="block w-full rounded-md border-1 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6"
                    />
                  </div>
                </div>
    
                <div>
                  <div className="flex items-center justify-between">
                    <label htmlFor="password" className="block text-sm font-medium leading-6 text-gray-900">
                      Password
                    </label>
                  </div>
                  <div className="mt-2">
                    <input
                      id="password"
                      name="password"
                      type="password"
                      required
                      className={errorMsg === "Passwords don't match" ? passwordClassName + " border-red-500" : passwordClassName}
                    />
                  </div>
                </div>
                <div>
                  <div className="flex items-center justify-between">
                    <label htmlFor="confirmedPassword" className="block text-sm font-medium leading-6 text-gray-900">
                      Confirm password
                    </label>
                  </div>
                  
                  <div className="mt-2">
                    <input
                      id="confirmedPassword"
                      name="confirmedPassword"
                      type="password"
                      required
                      className={errorMsg === "Passwords don't match" ? passwordClassName + " border-red-500" : passwordClassName}
                    />
                  </div>
                </div>
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