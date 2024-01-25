import { useEffect, useState } from "react";
import axios from "axios";
// import { isAdmin } from "./Login"
import { useParams } from "react-router-dom";
import ErrorBox from "../components/ErrorBox";
import { isAdmin } from "./Login";
import ButtonAccept from "../components/ButtonAccept";
import ButtonCancel from "../components/ButtonCancel";
import FormField from "../components/FormField";


export default function PatientEditPage() {
    const params = useParams();
    // let admin = isAdmin();
    const [patient, setPatient] = useState(Object());
    const [errorFlag, setErrorFlag] = useState(false);
    const [errorMsg, setErrorMsg] = useState("");

    let doctor = !isAdmin();

    useEffect(() => {
        axios.get(`/Patients/?id=${params.id}`).then(response => {
            setPatient(response.data);
        }).catch(error => console.log(error));
    }, [params]);

    const setError = (msg) => {
        setErrorMsg(msg);
        setErrorFlag(true);
    };

    const clearErrorFlag = () => {
        setErrorMsg("");
        setErrorFlag(false);
    };
    
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
            if (err.response.status === 409) {
                setError("Patient data was changed");
            } else {
                setError("Not found");
            }
        });
    };

    const handleDelete = () => {
        let patientPayload = {
            id: patient.id,
            concurrencyStamp: patient.concurrencyStamp,
        }//pobieramy i ustawiamy nowe concurrency stamp

        axios.post("/Patients/delete", patientPayload).then(response => {
            window.location.href = "/patients";
        }).catch(err => {
            console.log(err);
            if (err.response.status === 409) {
                setError("Patient data was changed");
            } else {
                setError("Not found");
            }
        });
    }
  
    return (
        <>
        <div className="flex min-h-full flex-1 flex-col justify-center px-6 py-12 lg:px-8">
          <div className="sm:mx-auto sm:w-full sm:max-w-sm">
            <h2 className="mt-10 text-left text-2xl font-bold leading-9 tracking-tight text-gray-900">
              {doctor ? "Patient" : "Edit patient"}
            </h2>
          </div>
  
          <div className="mt-5 sm:mx-auto sm:w-full sm:max-w-sm">
            <ErrorBox errorFlag={errorFlag} changeErrorFlag={clearErrorFlag} errorMsg={errorMsg}/>
            {/* <form className="space-y-6" action="#" method="POST"> */}
            <form className="space-y-6" onSubmit={handleSubmit}>
                <FormField type="text" attr="firstName" label="First name" defaultValue={patient.firstName} disabled={doctor}/>
                <FormField type="text" attr="surname" label="Surname" defaultValue={patient.surname} disabled={doctor}/>
                <FormField type="email" attr="email" label="Email address" defaultValue={patient.email} disabled={doctor}/>

              <div>
                <div className="flex items-center">
                  <input disabled={doctor} defaultChecked={patient.active} id="active" type="checkbox" value="" className="mr-2 w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500"/>
                  <label htmlFor="active" className="block text-sm font-medium leading-6 text-gray-900">
                      Active
                  </label>
                </div>
              </div>
                {!doctor &&
                    <div className="flex">
                        <ButtonAccept type="submit" className="w-6/12" text="Save"/>
                        <ButtonCancel className="w-6/12 ml-1" text="Delete" onClick={handleDelete}/>
                    </div>
                }
            </form>
          </div>
        </div>
      </>
    );
}