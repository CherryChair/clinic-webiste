import axios from "axios";
import { useState } from "react";
import { CheckIcon, XMarkIcon  } from '@heroicons/react/24/outline'

export default function PatientListElement({id, firstName, surname, email, activated, concurrencyStamp, setErrorFunc}) {
    const [active, setActive] = useState(activated);
    const [ccStump, setCcStump] = useState(concurrencyStamp);
    const activate = () => {
        let patientPayload = {
            id: id,
            firstName: firstName,
            surname: surname,
            email: email,
            active: !active,
            concurrencyStamp: ccStump,
        }//pobieramy i ustawiamy nowe concurrency stamp
        axios.post("/Patients/edit", patientPayload).then(response => {
            setActive(!active);
            setCcStump(response.data);
        }).catch(err => {
            console.log(err);
            if (err.response.status === 409) {
                setErrorFunc("Patient data was changed");
            } else {
                setErrorFunc("Not found");
            }
        });
    }
    return (
        <tr className="bg-white hover:bg-gray-50 ">
            <th scope="row" className="px-6 py-4 font-medium text-gray-900 whitespace-nowrap ">
                {firstName + " " + surname}
            </th>
            <td className="px-6 py-4">
                {email}
            </td>
            <td className="px-6 py-4">
                {active ? <CheckIcon  className="block h-6 w-6" aria-hidden="true" /> :
                    <XMarkIcon className="block h-6 w-6" aria-hidden="true" />
                }
            </td>
            <td className="px-6 py-4 text-right">
                <a href={"/patient/"+{id}} className="font-medium text-blue-600 hover:underline">Edit</a>
                {" "}
                <button onClick={activate} className="font-medium text-blue-600 hover:underline">{active ? "Deactivate" : "Activate"}</button>
                {" "}
                <button onClick={() => setErrorFunc("Test error")} className="font-medium text-blue-600 hover:underline">Delete</button>
            </td>
        </tr>
    );
}