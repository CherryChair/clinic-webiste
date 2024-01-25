import { useEffect, useState } from "react";
import axios from "axios";
// import { isAdmin } from "./Login"
import { useParams } from "react-router-dom";


export default function PatientEditPage() {
    const params = useParams();
    // let admin = isAdmin();
    const [patient, setPatient] = useState(Object());

    useEffect(() => {
        axios.get(`/Patients/?id=${params.id}`).then(response => {
            setPatient(response.data);
        }).catch(error => console.log(error));
    }, [params]);
    
    const handleSubmit = event => {
        event.preventDefault();
        let formData = event.target;
        let patientPayload = {
            id: patient.id,
            firstName: formData.firstName.value,
            surname: formData.surname.value,
            email: formData.email.value,
            active: formData.active.checked,
            concurrencyStamp: patient.concurrencyStamp,
        }
        axios.post("/Patients/edit", patientPayload).then(response => {
            let tempPat = patient;
            tempPat.concurrencyStamp = response.data;
            setPatient(tempPat);
        }).catch(err => {
            console.log(err);
        });
    };
  
    return (
        <>
        <div className="flex min-h-full flex-1 flex-col justify-center px-6 py-12 lg:px-8">
          <div className="sm:mx-auto sm:w-full sm:max-w-sm">
            <h2 className="mt-10 text-left text-2xl font-bold leading-9 tracking-tight text-gray-900">
              Edit patient
            </h2>
          </div>
  
          <div className="mt-5 sm:mx-auto sm:w-full sm:max-w-sm">
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
                    defaultValue={patient.firstName}
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
                    defaultValue={patient.surname}
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
                    defaultValue={patient.email}
                    autoComplete="email"
                    required
                    className="block w-full rounded-md border-1 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6"
                  />
                </div>
              </div>

              <div>
                <div className="flex items-center">
                  <input defaultChecked={patient.active} id="active" type="checkbox" value="" className="mr-2 w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500"/>
                  <label htmlFor="active" className="block text-sm font-medium leading-6 text-gray-900">
                      Active
                  </label>
                </div>
              </div>

              <div className="flex">
                <button
                  type="submit"
                  className="flex w-6/12 justify-center rounded-md bg-indigo-600 px-3 py-1.5 text-sm font-semibold leading-6 text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                >
                  Save
                </button>
                <button
                  className="flex ml-1 w-6/12 justify-center rounded-md bg-gray-500 px-3 py-1.5 text-sm font-semibold leading-6 text-white shadow-sm hover:bg-gray-400 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-gray-500"
                >
                  Delete
                </button>
              </div>
            </form>
          </div>
        </div>
      </>
    );
}