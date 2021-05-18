import ErrorMessageTag from './ErrorMessage';
import { ErrorMessage } from '@hookform/error-message';
import { camelToProper } from './Helpers';

function TextInput({ className, register, errors, id, text, type, readOnly = false }) {
  return (
    <>
      <label htmlFor={id} className="block font-medium text-gray-700">
        {text || camelToProper(id)}
      </label>
      <input
        type={type || 'text'}
        id={id}
        className={className}
        readOnly={readOnly}
        tabIndex={readOnly ? -1 : null}
        {...register(id)}
      />

      <ErrorMessage errors={errors} name={id} as={ErrorMessageTag} />
    </>
  );
}

export default TextInput;
