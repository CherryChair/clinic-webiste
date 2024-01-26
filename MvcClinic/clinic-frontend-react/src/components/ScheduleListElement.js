import axios from "axios";

export default function ScheduleListElement({id, doctorName, patientName, date, concurrencyStamp, setErrorFunc, onDelete}) {

    const deleteSchedule = () => {
        let schedulePayload = {
            id: id,
            concurrencyStamp: concurrencyStamp,
        }//pobieramy i ustawiamy nowe concurrency stamp

        axios.post("/Schedules/delete", schedulePayload).then(response => {
            onDelete();
        }).catch(err => {
            console.log(err);
            if (err.response.status === 409) {
                setErrorFunc("Schedule data was changed");
            } else {
                setErrorFunc("Not found");
            }
        });
    }

    return (
        <tr className="bg-white hover:bg-gray-50 ">
            <th scope="row" className="px-6 py-4 font-medium text-gray-900 whitespace-nowrap ">
                {date.replace("T", ", ")}
            </th>
            <td className="px-6 py-4">
                {doctorName}
            </td>
            <td className="px-6 py-4">
                {patientName}
            </td>
            <td className="px-6 py-4 text-right">
                <a href={"/schedule/"+ id } className="font-medium text-blue-600 hover:underline">Edit</a>
                {" "}
                <button onClick={deleteSchedule} className="font-medium text-blue-600 hover:underline">Delete</button>
            </td>
        </tr>
    );
}