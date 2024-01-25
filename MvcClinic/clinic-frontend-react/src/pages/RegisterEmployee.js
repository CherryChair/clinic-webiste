import axios from 'axios';
import { useState } from 'react';
import ErrorBox from '../components/ErrorBox';
import SuccessBox from '../components/SuccessBox';
import FormField from '../components/FormField';
import SpecialitiesFormField from '../components/SpecialitiesFormField';
import ButtonAccept from '../components/ButtonAccept';


function RegisterEmployeePage() {
    const [errorFlag, setErrorFlag] = useState(false);
    const [errorMsg, setErrorMsg] = useState("");
    const [successFlag, setSuccessFlag] = useState(false);
    const [successMsg, setSuccessMsg] = useState("");

    function handleSubmit(event) {
        event.preventDefault();
        if (event.target.password.value !== event.target.confirmedPassword.value) {
          setErrorFlag(true);
          setErrorMsg("Passwords don't match");
          return;
        }
        const registerPayload = {
          firstName: event.target.firstName.value,
          surname: event.target.surname.value,
          email: event.target.email.value,
          password: event.target.password.value,
          specializationId: event.target.specialityId.value,
        }
        console.log(registerPayload);
        axios.post("/Employees/register", registerPayload)
          .then(response => {
            setSuccess("Employee registered");
            clearErrorFlag();
          })
          .catch(err => {
            console.log(err);
            clearSuccessFlag();
            if (err.response.status === 409) {
              setError("Email registered");
            } else {
              setError("Server Error");
            }
        });
    };

    function setError(msg) {
      setErrorFlag(true);
      setErrorMsg(msg);
    }

    function setSuccess(msg) {
      setSuccessFlag(true);
      setSuccessMsg(msg);
    }

    function clearErrorFlag() {
      setErrorFlag(false);
      setErrorMsg("");
    }

    function clearSuccessFlag() {
      setSuccessFlag(false);
      setSuccessMsg("");
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
              <form className="space-y-6" onSubmit={handleSubmit}>
                <FormField type="text" attr="firstName" label="First name"/>
                <FormField type="text" attr="surname" label="Surname"/>
                <FormField type="email" attr="email" label="Email address"/>
                <SpecialitiesFormField/>
                <FormField type="password" attr="password" label="Password" 
                  className={errorMsg === "Passwords don't match" ? " border-red-500" : ""}
                />
                <FormField type="password" attr="confirmedPassword" label="Confirm password" 
                  className={errorMsg === "Passwords don't match" ? " border-red-500" : ""}
                />

                <ErrorBox errorFlag={errorFlag} changeErrorFlag={clearErrorFlag} errorMsg={errorMsg}/>
                <SuccessBox successFlag={successFlag} changeSuccessFlag={clearSuccessFlag} successMsg={successMsg}/>
                <div>
                  <ButtonAccept type="submit" text="Register"/>
                </div>
              </form>
            </div>
          </div>
        </>
      )
 }
 
 export default RegisterEmployeePage;