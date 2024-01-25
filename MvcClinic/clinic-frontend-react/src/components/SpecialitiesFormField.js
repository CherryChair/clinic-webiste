import axios from "axios";
import { useEffect, useState } from "react";

export default function SpecialitiesFormField({className, defaultValue}) {
    let css = "block w-full rounded-md border-1 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6";
    if (className) {
        css += className;
    }

    const [specialities, setSpecialities] = useState([]);

    useEffect(() => {
      getSpecialities();
    }, []);
    

    const getSpecialities = () => {
      axios.get("/Specialities/list").then(response => {
      setSpecialities(response.data);
      }).catch(error => console.log(error));
    }


    return (
      <div>
        <div className="flex items-center justify-between">
          <label htmlFor="specialityId" className="block text-sm font-medium leading-6 text-gray-900">
            Speciality
          </label>
        </div>
        <div className="mt-2">
          <select id="specialityId" defaultValue={defaultValue ? defaultValue : ""} className={css}>
            <option value="" key=""></option>
            {specialities.map((item) => (
              <option value={item.id} key={item.id}>{item.name}</option>
              ))}
          </select>
        </div>
      </div>
    );
}