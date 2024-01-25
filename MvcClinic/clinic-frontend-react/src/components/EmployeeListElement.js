import axios from "axios";
import { useState } from "react";
import { CheckIcon, XMarkIcon  } from '@heroicons/react/24/outline'

export default function EmployeeListElement({id, firstName, surname, email, speciality, concurrencyStamp, setErrorFunc, onDelete}) {

    const deleteEmployee = () => {
        let employeePayload = {
            id: id,
            concurrencyStamp: concurrencyStamp,
        }//pobieramy i ustawiamy nowe concurrency stamp

        axios.post("/Employees/delete", employeePayload).then(response => {
            onDelete();
        }).catch(err => {
            console.log(err);
            if (err.response.status === 409) {
                setErrorFunc("Employee data was changed");
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
                {speciality}
            </td>
            <td className="px-6 py-4 text-right">
                <a href={"/employee/"+ id } className="font-medium text-blue-600 hover:underline">Edit</a>
                {" "}
                <button onClick={deleteEmployee} className="font-medium text-blue-600 hover:underline">Delete</button>
            </td>
        </tr>
    );
}